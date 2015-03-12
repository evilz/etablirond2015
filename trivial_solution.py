from file_handler import Enum, readfile, writesoluce
(datacenter, servers, nbgroups) = readfile("input.txt")

# first fit

for (index, server) in enumerate(servers):
    match = False
    location = (-1, -1)
    for (x, row) in enumerate(datacenter):
        for (y, space) in enumerate(row):
            if space == Enum.EMPTY:
                match = True
                for j in range(server['size']):
                    if row[y + j] != Enum.EMPTY:
                        match = False
                if match:
                    location = (x, y)
                    break
        if match:
            break
    if match:
        for i in range(server['size']):
            datacenter[location[0]][location[1] + i] = index

# groups repartition

sortedServers = sorted(servers, key=lambda x: x['capacity'])

groups = list(0 for i in range(nbgroups))

for (index, server) in sortedServers:
    if sortedServers['used']:
        groupIndex = groups.index(min(groups))
        groups[groupIndex] += server['capacity']
        servers[server['index']]['group'] = groupIndex

writesoluce("output.txt", servers)
