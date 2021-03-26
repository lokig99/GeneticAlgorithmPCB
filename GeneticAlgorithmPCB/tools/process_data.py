import json
from typing import List
import matplotlib.pyplot as plt
import sys


with open(sys.argv[1], 'r') as file:
    data = json.load(file)['data']


averages: List[float] = []
worsts: List[float] = []
genBests: List[float] =[]

for gen in data:
    averages.append(gen['gAvg'])
    worsts.append(gen['wtF'])
    genBests.append(gen['fit'])

plt.plot(averages, label='generation average')
plt.plot(worsts, label='generation worst')
plt.plot(genBests, label='generation best')
plt.legend()
plt.xlabel("generation")
plt.ylabel("fitness")
plt.grid(True)
plt.show()


