local function myCallback(Toy, distance, callbackInfo , colliderInfoA, colliderInfoB, rawDistance) 
	print(distance)
	if (distance > 0) then 
		local value = (0.40 - distance) / 0.15

		Toy:setVibration(value)
	else 
		Toy:setVibration(0)
	end 
end

CTRL.Name = "Hush Binary Controller"
CTRL.Callbacks = {
	{
		Name = "Distance-based vibration",
		MinDistance = 0.40,
		Callback = myCallback,
	},
} 

