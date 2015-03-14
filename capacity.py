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
    return minCapacity

def getMinCapacity(datacenter, servs, nbGroups):
    minCapacity = 1000
    minGroup = None
    sum = 0

    for group in range(nbGroups):
        currCapacity = getMinCapacityByGroup(datacenter, servs, group)
        sum += currCapacity
        if minCapacity > currCapacity:
            minCapacity = currCapacity
            minGroup = group
            
    return minCapacity, minGroup, sum/45

def getNextToFit(datacenter, servs, groups):
    (minCapacity, minGroup, maxTheorical) = getMinCapacity(datacenter, servs, len(groups))
    
    #if minCapacity == 0:
    #    minGroup = groups.index(min(groups));
    
    minCapDc = 10000
    cap = 0
    for (x, row) in enumerate(datacenter):
        localServer = None
        maxCap = 0
        for id in set(row):
            if id != fh.Enum.EMPTY and id != fh.Enum.UNUSED:
                if servs[id]['group'] == minGroup:
                    cap += servs[id]['capacity']
                elif servs[id]['group'] == -1 and servs[id]['capacity'] > maxCap:
                    localServer = id
                    maxCap = servs[id]['capacity']
                        
        if cap < minCapDc and localServer != None:
            res = localServer
            minCapDc = cap
            
    return minGroup, res

if __name__ == "__main__":
  (datacenter, servs, nbGroups) = fh.readfile("./input.txt")
  print getMinCapacity(datacenter, servs, nbGroups)
