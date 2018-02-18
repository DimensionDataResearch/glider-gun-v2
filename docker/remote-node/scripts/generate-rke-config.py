#!/usr/bin/python


import click
import json

@click.command()
@click.option('--terraform-state-file', required=True)
def main(terraform_state_file=None):
    '''
    Generate RKE cluster configuration from Terraform outputs.

    The following outputs must be defined:
        - rke_control_plane_nodes
        - rke_etcd_nodes
        - rke_worker_nodes

    The outputs must be lists of node IP addresses
    (SSH-accessible from the system where RKE will be run).
    '''

    print("Terraform state file is '{0}'.".format(
        terraform_state_file
    ))

    with open(terraform_state_file) as state_file:
        terraform_state = json.load(state_file)

    outputs = {
        output_name: output['value']
        for module in terraform_state['modules']
        for (output_name, output) in module['outputs'].items()
    }

    rke_control_plane_nodes = []
    rke_etcd_nodes = []
    rke_worker_nodes = []
    for output in outputs.keys():
        if not output.startswith('rke_'):
            continue

        value = outputs[output]
        if not isinstance(value, list):
            value = [value]

        if output == 'rke_control_plane_nodes':
            rke_control_plane_nodes.extend(value)
        if output == 'rke_etcd_nodes':
            rke_etcd_nodes.extend(value)
        if output == 'rke_worker_nodes':
            rke_worker_nodes.extend(value)

    print('rke_control_plane_nodes = {0}'.format(
        repr(rke_control_plane_nodes)
    ))
    print('rke_etcd_nodes = {0}'.format(
        repr(rke_etcd_nodes)
    ))
    print('rke_worker_nodes = {0}'.format(
        repr(rke_worker_nodes)
    ))

    # TODO: Group nodes by role and generate cluster.yml.

if __name__ == '__main__':
    main()
