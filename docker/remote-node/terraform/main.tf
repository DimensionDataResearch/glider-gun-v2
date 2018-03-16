###########
# Variables

variable "image" {
  default = "ubuntu-16-04-x64"
}
variable "dns_subdomain" { /* provided elsewhere (e.g. TF_VAR_dns_subdomain environment variable) */ }
variable "kube_host_size" {
  default = "4gb" # AF: This can't be smaller than 2gb or RKE freaks out when Kubernetes runs out of memory.
}
variable "kube_host_count" {
  default = 3
}
variable "ssh_key_file" { /* provided elsewhere */ }
variable "ssh_public_key_file" { /* provided elsewhere */ }

# Our SSH key
resource "digitalocean_ssh_key" "kube_host" {
  name        = "kube-host.glider-gun.${var.dns_subdomain}"
  public_key  = "${file(var.ssh_public_key_file)}"
}

# The virtual server (Digital Ocean).
resource "digitalocean_droplet" "kube_host" {
  count     = "${var.kube_host_count}"

  image     = "${var.image}"
  name      = "kube-${count.index + 1}"
  region    = "nyc3"
  size      = "${var.kube_host_size}"

  ssh_keys  = [
    "${digitalocean_ssh_key.kube_host.fingerprint}"
  ]

  connection {
    type        = "ssh"
    user        = "root"
    private_key = "${file(var.ssh_key_file)}"
  }

  # Install Docker
  provisioner "remote-exec" {
    
    inline = [
      "apt-get update -qq",
      "apt-get install -q -y --no-install-recommends apt-transport-https curl software-properties-common",
      "apt-get install -q -y --no-install-recommends linux-image-extra-$(uname -r) linux-image-extra-virtual",
      "curl -fsSL 'https://sks-keyservers.net/pks/lookup?op=get&search=0xee6d536cf7dc86e2d7d56f59a178ac6c6238f52e' | sudo apt-key add -",
      "add-apt-repository -y \"deb https://packages.docker.com/1.12/apt/repo/ ubuntu-$(lsb_release -cs) main\"",
      "apt-get update -qq",
      "apt-get -q -y install docker-engine"
    ]
  }

  # Create host directories
  provisioner "remote-exec" {
    inline = [
      "mkdir -p /etc/glider-gun /var/run/glider-gun/state"
    ]
  }

  # Copy SSH key
  provisioner "file" {
    source      = "${var.ssh_key_file}"
    destination = "/etc/glider-gun/keys/ssh"
  }
}

# DNS record for the virtual server (CloudFlare).
resource "cloudflare_record" "kube_host" {
    count   = "${digitalocean_droplet.kube_host.count}"

    domain  = "tintoy.io"
    name    = "${element(digitalocean_droplet.kube_host.*.name, count.index)}.${var.dns_subdomain}"
    value   = "${element(digitalocean_droplet.kube_host.*.ipv4_address, count.index)}"
    type    = "A"
    ttl     = 120

    proxied = false
}

#########
# Outputs

output "kube_host_ips" {
  value = [
    "${digitalocean_droplet.kube_host.*.ipv4_address}"
  ]
}

output "kube_host_names" {
  value = [
    "${formatlist("%s.%s.tintoy.io", digitalocean_droplet.kube_host.*.name, var.dns_subdomain)}"
  ]
}

# Outputs used to generate RKE cluster manifest
output "rke_control_plane_nodes" {
  value = [
    "${element(digitalocean_droplet.kube_host.*.ipv4_address, 0)}"
  ]
}
output "rke_etcd_nodes" {
  value = [
    "${element(digitalocean_droplet.kube_host.*.ipv4_address, 0)}"
  ]
}
output "rke_worker_nodes" {
  value = [
    "${digitalocean_droplet.kube_host.*.ipv4_address}"
  ]
}

###########
# Providers

variable "do_token" { /* provided elsewhere */ }
provider "digitalocean" {
  token = "${var.do_token}"
}

variable "cloudflare_email" { /* provided elsewhere */ }
variable "cloudflare_token" { /* provided elsewhere */ }
provider "cloudflare" {
    email = "${var.cloudflare_email}"
    token = "${var.cloudflare_token}"
}
