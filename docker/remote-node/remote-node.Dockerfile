FROM tintoyddr.azurecr.io/glider-gun/base/ubuntu:xenial

RUN pip install click

ARG RKE_VERSION
ARG TERRAFORM_VERSION

RUN curl -L -o /usr/local/bin/rke https://github.com/rancher/rke/releases/download/${RKE_VERSION}/rke_linux-amd64 && \
    chmod a+x /usr/local/bin/rke

RUN curl -L -o /tmp/terraform.zip https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    unzip /tmp/terraform.zip && \
    rm /tmp/terraform.zip && \
    chmod a+x ./terraform && \
    mv ./terraform /usr/local/bin

COPY scripts/generate-rke-config.py scripts/entry-point.sh /usr/local/bin/
COPY ./terraform/* /deploy/

WORKDIR /deploy
RUN terraform init

ENTRYPOINT [ "/bin/bash", "/usr/local/bin/entry-point.sh" ]
