import os
for x in os.listdir('./localisation/'):
	if ".zhlocalisation" not in x:
		pass
	file = open("./localisation/"+x, "r", encoding="utf8").read().split('\n')
	i = 0
	for line in file:

			file[i] = ""
		i += 1
	
	newfile = open("./localisation/"+x, "w", encoding="utf8")
	newfile.write('\n'.join(file))