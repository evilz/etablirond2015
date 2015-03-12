#!/usr/bin/python

import file_handler as fh

def getGlobalCapacity(servs, group):
    capacity = 0
    for serv in servs:
         if serv['group'] == group:
             capacity += serv['capacity']
    return capacity

def getMinCapacityByGroup(datacenter, servs, group):
    globalCapacity = getGlobalCapacity(servs, group)
    minCapacity = globalCapacity

    for row in range(len(datacenter)):
        currCapacity = globalCapacity
        for serv in servs:
            if serv['group'] == group and serv['pos'][0] == row :
                currCapacity -= serv['capacity']
        minCapacity = min(minCapacity, currCapacity)
    print "capacity of: ", group, " ", minCapacity
    return minCapacity

def getMinCapacity(datacenter, servs, nbGroups):
    minCapacity = 1000

    for group in range(nbGroups):
        currCapacity = getMinCapacityByGroup(datacenter, servs, group)
        minCapacity = min(minCapacity, currCapacity)

    return minCapacity

if __name__ == "__main__":
  (datacenter, servs, nbGroups) = fh.readfile("./input.txt")
  print getMinCapacity(datacenter, servs, nbGroups)
