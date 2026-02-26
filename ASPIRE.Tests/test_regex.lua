local str = "9043288,0,1,3,0,18,12,1888,caldavar,02/22/2026,Hero_Jeraziah"
local _, _, id, result, team, kills, deaths, assists, heroid, duration, mapname, mdt, heroname = string.find(str, "(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+)")
print("ID:", id)
print("Result:", result)
print("HeroName:", heroname)
