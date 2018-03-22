# Glider Gun v2 - Design Notes

## Deployments

A deployment is an inherently stateful entity; we'll probably represent each deployment with its own actor that manages the deployment life-cycle. The deployment's status will be recorded in the database and treated as the source-of-truth (e.g. if the database says it's in-progress then you _can't_ run again it until it's marked as completed or failed, but you _can_ cancel it).

See [daas-demo](https://github.com/DimensionDataResearch/daas-demo) for [some](https://github.com/DimensionDataResearch/daas-demo/blob/master/src/DaaSDemo.Provisioning/Actors/TenantServerManager.cs) [examples](https://github.com/DimensionDataResearch/daas-demo/blob/master/src/DaaSDemo.Provisioning/Actors/TenantDatabaseManager.cs) of representing stateful activities using actors and database state.

## Jobs

Each deployment is represented in Kubernetes as a `Job`. The job's pod runs the template container, and the Glider Gun agent supplies it with initial state data / captures resulting state data and logs.

## Storage

Each pod representing a Glider Gun deployment needs 2 persistent volumes:

* `/state`  
  Holds input data (e.g. `template-parameters.json`) and state data produced by the deployment (e.g. `terraform.tfstate`).
* `/logs`  
  Holds log files (if any) produced by the deployment.

## Storage strategies

The Glider Gun cluster has multiple nodes, and the `/state` and `/log` volumes must be available on the node where the job is running. The contents of these volumes must be persisted between runs of the deployment (and therefore persisted somehow). There are 2 options to achieve this:

* Shared storage  
  A sub-folder of the shared storage volume is mounted into the pod's container.
* Local storage  
  A local folder is managed by the Glider Gun agent on each node, and this folder is then mounted into the pod as a local volume.

Given requirements for archiving and restoring state, the local storage option may be preferable (this can be done with shared storage but it's a little riskier in terms of race conditions).

### Shared storage

Could be NFS (either an existing NFS volume, or a Kubernetes `PersistentVolume` in `ReadWriteMany` mode using the NFS provisioner) or something like [Rook](https://rook.io/) dynamically provisioning the volume.

**Note:** currently, Rook doesn't seem to correctly handle `ReadWriteMany` (it barfs as soon as a volume is mounted into a second pod, even if it's marked as `ReadWriteMany`).

### Local storage

A local folder is created on the target node and populated with data as required (perhaps by an init container in the job pod).
When the job is complete, the folder contents are archived and persisted (perhaps in a database). If the deployment is run again, the previous folder contents are restored from the archive.

If we use an init container, then we can pass it an environment variable with a key used to retrieve the state and log archive streams. This would mean that one of the setup tasks for a job (run for the first time) would be to generate an archive with the required files and pre-populate the database with it.

How do we know when to archive the state and log volumes? When the job is complete? There's a potential race condition here - we'd need a way to lock the job state until archiving is complete. If we treat the Glider Gun deployment as the high-level entity (rather than the underlying Kubernetes job), then we can manage locking at that level (i.e. the deployment has its own lifecycle which can be used to determine whether a given action is permitted at any given time).

## Templates

In Glider Gun, a template consists of a Docker image, together with metadata describing the template's known inputs and outputs.

### Deploying a template

Generally, a template's base layers know how to import parameters.json into a format that the upper layers can interpret. For example, they may generate tfvars.json, and / or populate environment variables (perhaps with a prefix such as TF_VAR_). They may also know how to contact Vault and export secrets as environment variables (how does this work for remote nodes, though?).

Some system-level configuration options (e.g. Vault connection details) are always supplied as environment variables.

### Template types

A template type is a base image from which templates can be created.

There are 2 main kinds of template type:
	• Standard
	• Ad-hoc

Standard templates are built into images created from base images (standard template types), and their only inputs are template parameters. 

Ad-hoc templates are not built into images. Instead, they are mounted into a folder in a container created from a generic base image (an ad-hoc template type). Their inputs are therefore both template files and input parameters.

### Authoring standard templates

Select a template type (base image), and then add your template files. The template type may have one or more parameters, which are either optional or required.

Template authors can choose to delegate some or all parameters to the newly-created template. 

### Template builds

To build a standard template, the user selects a template type (i.e. base image), and then supplies metadata, configuration files and content files (either by uploading a .zip / .tgz, or by supplying the URL for a git repository). The system generates a Dockerfile to represent the template configuration (starts from the template-type base image and adds the template's configuration files / content files as required), then uses it to build and publish a new image representing the template.

### Template versioning

#### Template types

Template types consist of a Docker image and metadata (e.g. Parameters, IsAdHoc).

#### Standard Templates
Templates consist of source files and metadata. They can be imported from a zip / tarball / Git repository.

## Testing

* Test suite container(s)  
  Can be used to ensure that cluster and remote-node environments both provide the same functionality and constraints.

## Remote nodes

Something akin to a bastion host - if you want to run Ansible within someone's network you need to have an appliance there that can run jobs. Imagine something like Glider Gun v1, but polling for jobs / listening to a queue. Just Docker not Kubernetes. These nodes can, themselves, be deployed from templates. 

I considered simply adding remote nodes to the existing Kubernetes cluster but I think that might be a mistake, operationally - there are probably too many open ports and protocols required between the master and the worker to make this a viable strategy.

### Differences in execution environment

Using 2 different models for executing jobs does complicate the design somewhat - now Glider Gun templates may be run via Kubernetes or directly via the Docker API.

We'd need to ensure that templates are not specifically Kubernetes- or Docker-aware. Ideally, they should always be runnable as a single container with a couple of environment variables and mounted volumes.

Additionally, it means we need 2 different mechanisms (i.e. execution adapters) for invoking a job and managing / capturing its state: in the main (Kubernetes) cluster, we'd use a Job object, and on remote nodes we'd have to create and manage the container directly.

#### Docker-in-Docker (DinD)

Another possibility could be using Docker-in-Docker (DinD) from both Kubernetes (pod has Glider Gun Agent container and DinD container, with agent talking to DinD's Docker API) and directly from Docker (Glider Gun Agent container and DinD container are hosted directly in Docker, and Glider Gun Agent talks to DinD's Docker API) to execute containers. This way, a Glider Gun agent will always talk to local Docker wherever it is running (regardless of whether that's a local or remote node).

#### Single-node Rancher Kubernetes Engine (RKE)

This has the advantage that even remote nodes are still Kubernetes and we can have a consistent API / workflow. Minor disadvantage - must disable swap (and provide >=8GB of RAM) for K8s to work correctly.

```yaml
#
# Single-node Kubernetes cluster using Rancher Kubernetes Engine (RKE)
#

---
nodes:
  - address: 192.168.17.20
    user: root
    role:
      - controlplane
      - etcd
      - worker

services:
  etcd:
    image: quay.io/coreos/etcd:latest
  kube-api:
    image: rancher/k8s:v1.8.3-rancher2
  kube-controller:
    image: rancher/k8s:v1.8.3-rancher2
  scheduler:
    image: rancher/k8s:v1.8.3-rancher2
  kubelet:
    image: rancher/k8s:v1.8.3-rancher2
  kubeproxy:
    image: rancher/k8s:v1.8.3-rancher2
```

### Remote node deployment

Remote nodes are linux appliances (VMs, most likely) that are configured using an Ansible playbook (to install / configure the required components).

They run Linux with Docker; all required services (e.g. Glider Gun Remote, Vault) are run as Docker containers.

We could use a deployment template to deploy remote nodes :-)

### Cluster-to-remote-node communications

Remote nodes connect to the cluster, never the other way around.

Options:

* SignalR / WAMP
* HTTP
* ZeroMQ

#### SignalR / WAMP

First preference.

* Relatively simple to implement, and stateful (matches up nicely to Akka.NET actors).
* In this model, the cluster's API gateway acts as a hub; nodes can receive notifications for jobs, and publish notifications for job results.
* Use mutual SSL for authentication.

#### HTTP

Second preference.

* Simplest to implement, but stateless (so state has to be managed elsewhere).
* In this model, remote nodes call the cluster's API gateway to retrieve configuration, poll for jobs and publish job results.
* Use mutual SSL for authentication.

#### ZeroMQ

Third preference.

* May be more complex to implement (less of a known-quantity).
* Use ZeroMQ for pub / sub. Might be complicated by the need to have all connections only occur in a single direction (remote nodes -> cluster API gateway).
* Use mutual SSL for authentication.

#### Bootstrapping authentication

Need a way to get remote node and cluster to trust each other's certificates. If / when nodes are deployed via Glider Gun, then this could be accomplished using template parameters.

### Secret management on remote nodes

* How do remote nodes manage secrets?
* Since they already run Docker, should they also run a Vault container (perhaps in-memory only)?  
  Secrets in this Vault instance would only be populated for the duration of their corresponding jobs.
* Containers could therefore run the same way regardless of whether they're on the main cluster or a remote node.
