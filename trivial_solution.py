from file_handler import Enum, readfile, writesoluce
from capacity import getMinCapacity, getNextToFit

(datacenter, servers, nbgroups) = readfile("input.txt")

# first fit

fitServers = sorted(servers, key=lambda x: (float)(x['size']) / (float)(x['capacity']))
capServers = sorted(servers, key=lambda x: -x['capacity'])

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
        servers[server['id']]['datacenter'] = datacenter.index(row)
        for i in range(server['size']):
            datacenter[location[0]][location[1] + i] = server['id']

# groups repartition
# fuse
#for server in fitServers:
#    if server['used']:
#        server['group'] = 0
        
groups = list(0 for i in range(nbgroups))

for groupIndex in range(nbgroups):
    rowList = list()
    while groups[groupIndex] < 410:
        for server in capServers:
            if server['used'] and server['group'] == -1 and server['datacenter'] not in rowList:
                selectedServer = server
                break
                
        groups[groupIndex] += selectedServer['capacity']
        selectedServer['group'] = groupIndex
        rowList.append(selectedServer['datacenter'])
        #print "Server {} assigned to pool {}".format(selectedServer['id'], groupIndex)
         
while True:
    needToBreak = True
    for server in fitServers:
        if server['used'] and server['group'] == -1:
            needToBreak = False
            continue

    if needToBreak:
        break
        
    (groupId, serverId) = getNextToFit(datacenter, servers, groups)
    groups[groupId] += servers[serverId]['capacity']
    servers[serverId]['group'] = groupId
    #print "Server {} assigned to pool {}".format(serverId, groupId)

writesoluce("DatacenterOptimizer\DatacenterOptimizer\placement.in", servers)

print getMinCapacity(datacenter, servers, nbgroups)
