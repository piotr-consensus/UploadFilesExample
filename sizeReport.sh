#!/bin/bash

# Prints out the size of running containers every second
while true
do
    docker ps --size --format '{{json .Size}}'
    sleep 1
done
