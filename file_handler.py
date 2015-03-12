#!/usr/bin/python

class Enum():

  """Enum for datacenter"""

  def __init__(self):
    """TODO: to be defined1. """
    pass

  UNUSED = -2
  EMPTY = -1
    

def readfile(file):
  with open(file, "r") as f:
   (row, row_size, unusable, groups, numServers) = [int(x) for x in f.readline().split()]
   datacenter = list(list(Enum.EMPTY for i in range(row_size)) for j in range(row))
   servers = list(dict())
#unusable slots
   for i in range(unusable):
     (unusableX, unusableY) = [int(x) for x in f.readline().split()]
     datacenter[unusableX][unusableY] = Enum.UNUSED
#servers
   for i in range(numServers):
     (serverSize, serverCap) = [int(x) for x in f.readline().split()]
     servers.append({'size' : serverSize, 'capacity':serverCap, 'group':-1, 'pos':(-1,-1)})

   return (datacenter, servers)

def writesoluce(soluce):
  pass


if __name__ == "__main__":
  (dc, serv) = readfile("./test.txt")
  print dc
  print serv
