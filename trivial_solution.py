from file_handler import Enum, readfile, writesoluce
from capacity import getMinCapacity

(datacenter, servers, nbgroups) = readfile("input.txt")

# first fit

fitServers = sorted(servers, key=lambda x: (float)(x['size']) / (float)(x['capacity']))

for server in fitServers:
    match = False
    location = (-1, -1)
    fitRows = sorted(enumerate(datacenter), key=(lambda x: sum(servers[i]['capacity'] for i in set(x[1]) if i != Enum.EMPTY and i != Enum.UNUSED)))
    i = 0
    for (x, row) in fitRows:
        i += 1
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
