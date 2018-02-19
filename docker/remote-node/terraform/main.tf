###########
# Variables

variable "image" {
  default = "ubuntu-16-04-x64"
}
variable "kube_host_size" {
  default = "4gb" # AF: This can't be smaller than 2gb or RKE freaks out when Kubernetes runs out of memory.
}
variable "kube_host_count" {
  default = 3
}
variable "ssh_key_fingerprints" {
  default = [
    "5a:7b:99:36:e8:86:e0:4f:0f:7a:ce:cf:cb:00:b9:61"
  ]
}
variable "ssh_key_file" { /* provided elsewhere */ }

# The virtual server (Digital Ocean).
resource "digitalocean_droplet" "kube_host" {
  count     = "${var.kube_host_count}"

  image     = "${var.image}"
  name      = "kube-${count.index + 1}"
  region    = "nyc3"
  size      = "${var.kube_host_size}"

  ssh_keys  = [
    "${var.ssh_key_fingerprints}"
  ]

  connection {
    type        = "ssh"
    user        = "root"
    private_key = "${file(var.ssh_key_file)}"
  }

  provisioner "remote-exec" {
    # Install Docker
    inline = [
      "apt-get update",
      "apt-get install -y --no-install-recommends apt-transport-https curl software-properties-common",
      "apt-get install -y --no-install-recommends linux-image-extra-$(uname -r) linux-image-extra-virtual",
      "curl -fsSL 'https://sks-keyservers.net/pks/lookup?op=get&search=0xee6d536cf7dc86e2d7d56f59a178ac6c6238f52e' | sudo apt-key add -",
      "add-apt-repository -y \"deb https://packages.docker.com/1.12/apt/repo/ ubuntu-$(lsb_release -cs) main\"",
      "apt-get update",
      "apt-get -y install docker-engine"
    ]
  }
}

# DNS record for the virtual server (CloudFlare).
resource "cloudflare_record" "kube_host" {
    count   = "${digitalocean_droplet.kube_host.count}"

    domain  = "tintoy.io"
    name    = "${element(digitalocean_droplet.kube_host.*.name, count.index)}.yo-dawg"
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
    "${formatlist("%s.yo-dawg.tintoy.io", digitalocean_droplet.kube_host.*.name)}"
  ]
}

output "rke_control_plane_nodes" {
  value = [
    "${digitalocean_droplet.kube_host.*.ipv4_address}"
  ]
}
output "rke_etcd_nodes" {
  value = [
    "${digitalocean_droplet.kube_host.*.ipv4_address}"
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
