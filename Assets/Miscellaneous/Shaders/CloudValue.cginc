
#ifndef CLOUDS
#define CLOUDS
#include "SimpleValueNoise.cginc"
#include "IqNoise.cginc"

float2 _baseOffset0;
float2 _baseOffset1;
float _coverageOffset;

void fractalnate(float2 position, float scale, out float3 val) {
	float ampl = 1.0;
	val = 0.0;
	for (int i = 0; i < 3; i++) {
		float output = 0.0;
		float3 raw = noised(position * scale * 0.3);
		raw.x = raw.x * 0.5;
		//Unity_SimpleNoise_float(position, scale, output);
		val += raw * ampl * 0.8;
		ampl *= 0.4;
		scale *= 2.5;
	}
}


void doTheThing(float2 position, bool shadowed, out float alpha, out float2 derivative) {
	float pixelization = 200;
	//float pixelization = shadowed ? 1.0 : 20;
	float separation = 0.03;
	float base = 0.5;
	float scale = 0.0001;
	

	if (!shadowed) {
		position = round(position / pixelization) * pixelization;
	}

	//Unity_SimpleNoise_float(position, scale, value);
	float3 value = 0.0;
	fractalnate(position, scale, value);

	float3 value2 = 0.0;
	fractalnate(position, scale * 0.2, value2);
	value.x *= (value2.x + 0.4);

	derivative = value.yz;
	alpha = smoothstep(base - separation, base + separation, value.x + _coverageOffset * 0.25 + 0.35);
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

float light(float2 deriv, float3 sun, float3 ray) {
	float3 normal = normalize(float3(deriv.x, 1, deriv.y));
	float d = clamp(dot(sun, normal) * 0.5 + 0.5, 0, 1);
	float s = pow(max(dot(sun, ray), 0), 50);

	float strength = 1.0 * (1 - abs(sun.y) * 0.3);

	float ambient = clamp(-sun.y * 3.5 + 0.2, 0.03, 1);

	return lerp(d * strength + (1 - strength), 1.0, s) * ambient;
}


void sampleAllClouds_float(float3 ray_start, float3 ray_dir, float3 sun, bool shadowed, out float alpha, out float colour) {
	if (shadowed) {
		//ray_start = round(ray_start / 3) * 3;
	}

	float alpha1 = 1.0;
	float2 derivative1 = 0.0;
	doTheThing(cloudinate(ray_start, ray_dir, 2000, _baseOffset0), shadowed, alpha1, derivative1);
	alpha = alpha1;
	float light1 = light(derivative1, sun, ray_dir);

	float alpha2 = 1.0;
	float2 derivative2 = 0.0;
	doTheThing(cloudinate(ray_start, ray_dir, 3000, _baseOffset1), shadowed, alpha2, derivative2);
	alpha += alpha2;
	float light2 = light(derivative2, sun, ray_dir);

	//alpha = abs(derivative1.x) + 0.1;
	alpha = clamp(alpha, 0, 1);

	float remaining = 1.0;
	colour = alpha1 * light1;
	remaining -= alpha1;

	colour += alpha2 * light2 * remaining;
	//colour = (light1 * alpha1 + light2 * alpha2) / (alpha1 + alpha2);
	//colour = light1 * (1.0 - alpha2) + light2 * alpha2;
}

#endif