from file_handler import Enum, readfile, writesoluce
from capacity import getMinCapacity, getMinCapacityByGroup
import pickle

(datacenter, servers, nbgroups) = readfile("input.txt")

# first fit

fitServers = sorted(servers, key=lambda x: (float)(x['size']) / (float)(x['capacity']))

pickle.dump(fitServers, open("tamere", "w"))
pickle.dump(servers, open("tasoeur", "w"))

for (index, server) in enumerate(fitServers):
    match = False
    location = (-1, -1)
    fitDatacenter = sorted(enumerate(datacenter), key=lambda x: sum(1 for i in x[1] if i == Enum.EMPTY))
    for (x, row) in fitDatacenter:
        #y = 0
        #bestY = None
        #bestSize = 1000
        #while y < len(row):
            #if row[y] == Enum.EMPTY:
                #size = 0
                #while y < len(row) and row[y] == Enum.EMPTY:
                    #size += 1
                    #y += 1
                #if size < bestSize and size >= server['size']:
                    #bestY = y - size
                    #bestSize = size
                    #match = True
            #y += 1
        #if match:
            #location = (x, bestY)
            #break
        for (y, space) in enumerate(row):
            if space == Enum.EMPTY:
                match = True
                bestY = None
                bestSize = 1000
                for j in range(server['size']):
                    if y + j < len(row):
                        if row[y + j] != Enum.EMPTY:
                            match = False
                    else:
                        match = False
                if match:
                    location = (x, y)
                    break
        if match:
            break
    if match:
        servers[server['id']]['used'] = True
        servers[server['id']]['pos'] = location
        for i in range(server['size']):
            datacenter[location[0]][location[1] + i] = server['id']

# groups repartition

sortedServers = sorted(servers, key=lambda x: x['capacity'])

groups = list(0 for i in range(nbgroups))

for row in datacenter:
    for id in set(row):
        if id != Enum.EMPTY and id != Enum.UNUSED:
            groupIndex = groups.index(min(groups))
            groups[groupIndex] += servers[id]['capacity']
            servers[id]['group'] = groupIndex

for i in range(1):
    capacitiesGroup = list((getMinCapacityByGroup(datacenter, servers, group), group) for group in range(nbgroups))
    capacitiesGroup.sort(key=lambda x:x[0])
    minGroup = capacitiesGroup[0]
    maxGroup = capacitiesGroup[-1]
    print minGroup, maxGroup
    fitDatacenter = [(index, sum(1 for i in set(row) if servers[i]['group'] == minGroup[1])) for index, row in enumerate(datacenter)]
    fitDatacenter.sort(key=(lambda x: x[1]), reverse=True)
    minServerId = None
    minServerSize = None
    maxServerId = None
    found = 0
    for server in servers:
        if server['group'] == minGroup[1] and server['pos'][0] == fitDatacenter[0][0]:
            if found == 3:
                minServerId = server['id']
                minServerSize = server['size']
                break
            found += 1
    for server in servers:
        if server['group'] == maxGroup[1] and server['pos'][0] != fitDatacenter[0][0] and server['size'] == minServerSize:
            maxServerId = server['id']
            break

    print minServerId, minServerSize, maxServerId

    if maxServerId is not None and minServerId is not None:
        servers[maxServerId]['group'] = minGroup
        servers[minServerId]['group'] = maxGroup

writesoluce("output.txt", servers)

print getMinCapacity(datacenter, servers, nbgroups)
