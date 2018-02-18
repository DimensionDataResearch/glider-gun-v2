#!/usr/bin/python


import click

@click.command()
@click.option('--terraform-state-file', required=True)
def main(terraform_state_file=None):
    print("Terraform state file is '{0}'.".format(
        terraform_state_file
    ))

    # TODO: Read outputs and generate cluster.yml.

if __name__ == '__main__':
    main()
