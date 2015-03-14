from file_handler import Enum, readfile, writesoluce
from capacity import getMinCapacity


def findSpace(server, row):
    '''
    Returns the location of the first empty spot of the first big enough space
    for server in row
    '''
    match = False
    for (y, space) in [(i, x) for (i, x) in enumerate(row) if x == Enum.EMPTY]:
        match = True
        for j in range(server['size']):
            if y + j < len(row):
                if row[y + j] != Enum.EMPTY:
                    match = False
                    break
            else:
                match = False
                break
        if match:
            return y
    return None


def allocateServers(datacenter, servers, fitServers):
    '''
    Here starts the greedy allocation of servers in the datacenter. The ones
    with the highest densities are allocated first. The rows for each server
    are prioritized in increasing order of current total capacity.

    This repartition is somewhat equitable, and results in zero fragmentation.
    It maximizes the total capacities of rows.
    '''
    for server in fitServers:
        match = False
        location = (-1, -1)
        # Order rows by increasing order of total capacity
        fitRows = sorted(enumerate(datacenter),
                         key=(lambda x: sum(servers[i]['capacity']
                                            for i in set(x[1])
                                            if i != Enum.EMPTY and i != Enum.UNUSED)))
        for (x, row) in fitRows:
            y = findSpace(server, row)
            if y is not None:
                location = (x, y)
                match = True
                break
        if match:
            servers[server['id']]['used'] = True
            servers[server['id']]['pos'] = location
            for i in range(server['size']):
                datacenter[location[0]][location[1] + i] = server['id']


def allocateGroups(datacenter, servers, groups):
    capServers = sorted(servers,
                        key=lambda x: x['capacity'],
                        reverse=True)

    # fill each group with biggest capacities first until limit
    for groupIndex, group in enumerate(groups):
        rowList = list()
        while groups[groupIndex] < 400:  # limit
            for server in capServers:
                if server['used'] and server['group'] == -1 and server['pos'][0] not in rowList:
                    selectedServer = server
                    break

            groups[groupIndex] += selectedServer['capacity']
            selectedServer['group'] = groupIndex
            rowList.append(selectedServer['pos'][0])


def getNextToFit(datacenter, servs, groups):
    (minCapacity, minGroup, maxTheorical) = getMinCapacity(datacenter, servs, len(groups))

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

        if capInDc < minCap and biggestServerInCapDc is not None:
            minCap = capInDc
            if maxCapInDc > maxCap:
                biggestServerInCap = biggestServerInCapDc
                maxCap = maxCapInDc

    return minGroup, biggestServerInCap


if __name__ == "__main__":
    (datacenter, servers, nbGroups) = readfile("input.txt")

    # sort the servers by decreasing ratio of "capacity / size" (i.e. their density)
    fitServers = sorted(servers,
                        key=lambda x: (float)(x['capacity']) / (float)(x['size']),
                        reverse=True)
    groups = list(0 for i in range(nbGroups))

    allocateServers(datacenter, servers, fitServers)
    allocateGroups(datacenter, servers, groups)

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

    print getMinCapacity(datacenter, servers, nbGroups)
