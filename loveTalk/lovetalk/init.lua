print("Lovetalk")


print("SendToMoon()");
print("Passed execution to lua state!")



print("Testing bluetooth...");
local wb = BTManager.getDevices();
for I=0, wb.Length-1 do 
	local yes = wb[I]
	local toy = loveToy.connectCreateDevice(yes); 
	print(toy:getInfo())
	toy:setVibration(4)
end

