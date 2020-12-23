local function vibrationCallback(Toy, distance, callbackInfo , colliderInfoA, colliderInfoB, rawDistance) 
	if (distance > 0) then 
		local value = (0.40 - distance) / 0.15
		Toy:setVibration(value)
	else 
		Toy:setVibration(0)
	end 
end

local function squeezeCallback(Toy, distance, callbackInfo , colliderInfoA, colliderInfoB, rawDistance) 
	if (distance > 0) then 
		local value = (0.40 - distance) / 0.15 -- Dynamic bone radial distance , exclude center 15% while maintaining minimum radius, makes for a steeper ramp. 
		Toy:setAirLevelAbsolute(1)
	else 
		Toy:setAirLevelAbsolute(0)
	end 
end


CTRL.Name = "Lovense Max Distance-Variant Controller"
CTRL.Callbacks = {
	{
		Name = "Distance-based vibration",
		MinDistance = 0.40,
		Callback = vibrationCallback,
	},

	{
		Name = "Distance-based Squeeze",
		MinDistance = 0.40,
		Callback = squeezeCallback,
	},
} 




