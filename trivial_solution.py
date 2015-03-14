from file_handler import Enum, readfile, writesoluce
from capacity import getMinCapacity

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
groups = list(0 for i in range(nbgroups))

# fill each group with biggest capacities first until limit
for groupIndex in range(nbgroups):
    rowList = list()
    while groups[groupIndex] < 405: #limit
        for server in capServers:
            if server['used'] and server['group'] == -1 and server['datacenter'] not in rowList:
                selectedServer = server
                break
                
        groups[groupIndex] += selectedServer['capacity']
        selectedServer['group'] = groupIndex
        rowList.append(selectedServer['datacenter'])

def getNextToFit(datacenter, servs, groups):
    (minCapacity, minGroup, maxTheorical) = getMinCapacity(datacenter, servs, len(groups))
    
    #if minCapacity == 0:
    #    minGroup = groups.index(min(groups));
    
    # Extract biggest server available from the lowest dc we occupy
    maxCap = 0
    minCap = 10000
    biggestServerInCap = None
    
    for row in datacenter:
        biggestServerInCapDc = None
        capInDc = 0
        maxCapInDc = 0
        
        for id in set(row):
            if id != Enum.EMPTY and id != Enum.UNUSED:
                if servs[id]['group'] == minGroup:
                    capInDc += servs[id]['capacity']
                elif servs[id]['group'] == -1 and servs[id]['capacity'] > maxCapInDc:
                    biggestServerInCapDc = id
                    maxCapInDc = servs[id]['capacity']
        
        # Put random in here (it can be equal to another one)        
        if capInDc < minCap and biggestServerInCapDc != None and maxCapInDc > maxCap:
            minCap = capInDc
            biggestServerInCap = biggestServerInCapDc
            maxCap = maxCapInDc
            
    return minGroup, biggestServerInCap

while True:
    # if not available server, break
    needToBreak = True
    for server in fitServers:
        if server['used'] and server['group'] == -1:
            needToBreak = False
            continue

    if needToBreak:
        break

    # get next server and group to fit
    (groupId, serverId) = getNextToFit(datacenter, servers, groups)
    groups[groupId] += servers[serverId]['capacity']
    servers[serverId]['group'] = groupId

writesoluce("DatacenterOptimizer\DatacenterOptimizer\placement.in", servers)

print getMinCapacity(datacenter, servers, nbgroups)
