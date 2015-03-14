from file_handler import Enum, readfile, writesoluce
from capacity import getMinCapacity

(datacenter, servers, nbgroups) = readfile("input.txt")

# first fit

fitServers = sorted(servers, key=lambda x: (float)(x['size']) / (float)(x['capacity']))
datacenterIndexer = 0

for (index, server) in enumerate(fitServers):
    match = False
    location = (-1, -1)
    i = 0
    while i < len(datacenter):
        i += 1
        x = datacenterIndexer%len(datacenter)
        row = datacenter[datacenterIndexer%len(datacenter)]
        datacenterIndexer += 1
        for (y, space) in enumerate(row):
            if space == Enum.EMPTY:
                match = True
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

groups = list(0 for i in range(nbgroups))

for row in datacenter:
    for id in set(row):
        if id != Enum.EMPTY and id != Enum.UNUSED:
            groupIndex = groups.index(min(groups))
            groups[groupIndex] += servers[id]['capacity']
            servers[id]['group'] = groupIndex

writesoluce("DatacenterOptimizer\DatacenterOptimizer\placement.in", servers)

print getMinCapacity(datacenter, servers, nbgroups)
