FROM ubuntu:xenial

VOLUME [ "/secrets", "/state", "/logs" ]

RUN apt-get update && \
    apt-get install -y python python-pip jq curl unzip && \
    apt-get autoremove -y && \
    apt-get clean && \
    pip install --upgrade pip pyaml requests

ARG VAULT2ENV_VERSION

RUN curl -L -o /tmp/vault2env.zip https://github.com/DimensionDataResearch/vault2env/releases/download/${VAULT2ENV_VERSION}/vault2env.${VAULT2ENV_VERSION}.linux-amd64.zip && \
    unzip /tmp/vault2env.zip && \
    rm /tmp/vault2env.zip && \
    chmod a+x vault2env && \
    mv vault2env /usr/local/bin
