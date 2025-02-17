
#ifndef CLOUDS
#define CLOUDS
#include "SimpleValueNoise.cginc"

float2 _baseOffset0;
float2 _baseOffset1;
float _coverageOffset;

void fractalnate(float2 position, float scale, out float val) {
	float ampl = 1.0;
	for (int i = 0; i < 3; i++) {
		float output = 0.0;
		Unity_SimpleNoise_float(position, scale, output);
		val += output * ampl * 0.7;
		ampl *= 0.4;
		scale *= 2.5;
	}
}


void doTheThing(float2 position, bool shadowed, out float alpha) {
	float pixelization = 20;
	//float pixelization = shadowed ? 1.0 : 20;
	float separation = 0.05;
	float base = 0.5;
	float scale = 0.01;
	

	if (!shadowed) {
		position = round(position / pixelization) * pixelization;
	}

	//Unity_SimpleNoise_float(position, scale, value);
	float value = 0.0;
	fractalnate(position, scale, value);

	float value2 = 0.0;
	fractalnate(position, scale * 0.2, value2);
	value *= (value2 + 0.4);

	alpha = smoothstep(base - separation, base + separation, value + _coverageOffset);
}

void intersectPlane(float3 position, float3 normal, float3 ray_start, float3 ray_dir, out bool hit, out float3 target_position) {
	float3 origin = position - ray_start;
	float dotted = dot(origin, normal) / dot(ray_dir, normal);
	target_position = ray_dir * dotted + ray_start;
	hit = dotted >= 0.0;
}

float2 cloudinate(float3 position, float3 sun_direction, float height, float2 offset) {
	bool hit;
	float3 final_position;
	intersectPlane(float3(0, height, 0), float3(0, 1, 0), position, -sun_direction, hit, final_position);
	return hit ? (final_position.xz + unity_noise_randomValue(float2(height, 1123.321)) * 11232.231 + offset) : 0.0; 
}



void sampleAllClouds_float(float3 ray_start, float3 ray_dir, bool shadowed, out float alpha) {
	if (shadowed) {
		//ray_start = round(ray_start / 3) * 3;
	}

	float alpha1 = 1.0;
	doTheThing(cloudinate(ray_start, ray_dir, 350, _baseOffset0), shadowed, alpha1);
	alpha = alpha1;

	float alpha2 = 1.0;
	doTheThing(cloudinate(ray_start, ray_dir, 550, _baseOffset1), shadowed, alpha2);
	alpha += alpha2;

	alpha = clamp(alpha, 0, 1);
}

#endif