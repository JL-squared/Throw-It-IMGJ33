
#ifndef CLOUDS
#define CLOUDS

#include "SimpleValueNoise.cginc"
void doTheThing(float2 position, out float alpha) {
	float value = 0.0;
	float pixelization = 20;
	float separation = 0.05;
	float base = 0.5;
	float scale = 0.02;
	
	position = round(position / pixelization) * pixelization;

	Unity_SimpleNoise_float(position, scale, value);

	alpha = smoothstep(base - separation, base + separation, value);
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

float2 baseOffset;

void sampleAllClouds_float(float3 ray_start, float3 ray_dir, out float alpha) {
	float alpha1 = 1.0;
	doTheThing(cloudinate(ray_start, ray_dir, 400, baseOffset), alpha1);
	alpha = alpha1;

	/*
	float alpha2 = 1.0;
	doTheThing(cloudinate(ray_start, ray_dir, 40), alpha2);
	alpha += alpha2;
	*/
}

#endif