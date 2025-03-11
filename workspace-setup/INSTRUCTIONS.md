# Curiosity Workspace Installation Guide

Welcome to the Curiosity Workspace Installation Guide! This document will walk you through the process of installing Curiosity Workspace. By the end of this guide, you’ll have a fully operational Curiosity Workspace on your local machine.

Whether you're using Windows or Docker, this guide has you covered with step-by-step installation instructions. From there, you'll learn how to write a data connector, define node and edge schemas, and ingest JSON-based datasets into the system. Finally, we'll show you how to explore and query the data using Curiosity Workspace’s powerful shell interface.

Let’s get started!

## Table of Contents
1. [Installation](#installation)
2. [Initial Setup](#initial-setup)

---

## Installation
Install a Curiosity Workspace to your system using the following instructions:

### Windows

- Download the Curiosity Workspace for Windows install file (here)[https://downloads.curiosity.ai/workspace/windows]
- Run the setup, and follow the instructions to install.
- Once installed, open your start menu and run the "Curiosity Workspace" app

### Docker:

You must configure the values that are required for a standard installation. Unlike in the Installation guide, you cannot use a YAML file in the root folder of the app, therefore pass the configuration variables directly to the docker run command:

```bash
mkdir -p ~/curiosity/storage
docker run -p 8080:8080 -v ~/curiosity/storage/:/data/ -e storage=/data/curiosity
```

If you're running on Windows, you will need to adapt the paths as required:

```bash
mkdir c:\curiosity\storage
docker run -p 8080:8080 -v c:/curiosity/storage/data/:/data/ -e storage=/data/curiosity
```

## Initial Setup

Navigate to your workspace on your browser (http://localhost:8080) and login with with user and password `admin`. Follow the steps to give your workspace a name, and proceed till the end.


## Next steps
- Create a data connector for the provided dataset in the [Data Connector Guide](/data-connector/INSTRUCTIONS.md)
- Configure natural language processing parsing in the [NLP Configuration Guide](/nlp-configuration/INSTRUCTIONS.md)
- Configure search on the data in the [Search Configuration Guide](/search-configuration/INSTRUCTIONS.md)
