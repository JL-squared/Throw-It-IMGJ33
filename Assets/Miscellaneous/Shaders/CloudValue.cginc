
#ifndef CLOUDS
#define CLOUDS
#include "IntersectPlane.cginc"
#include "SimpleValueNoise.cginc"
#include "IqNoise.cginc"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SphericalHarmonics.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"

float2 _baseOffset0;
float2 _baseOffset1;
float _coverageOffset;

void fractalnate(float2 position, float scale, out float3 val) {
	float ampl = 1.0;
	val = 0.0;
	for (int i = 0; i < 5; i++) {
		float output = 0.0;
		float3 raw = noised(position * scale * 0.3);
		raw.x = raw.x * 0.5;
		//Unity_SimpleNoise_float(position, scale, output);
		val += raw * ampl * 0.8;
		ampl *= 0.4;
		scale *= 2.5;
	}

	val += noised(position * 0.002 * float2(3, 1)) * 0.02;
	val += 0.05;
	val.x *= saturate(noised(position * scale * 0.003)*3 + 1);
}
SAMPLER(my_point_clamp_sampler);

void doTheThing(float2 position, bool shadowed, out float alpha, out float2 derivative) {
	float pixelization = 100;
	//float pixelization = shadowed ? 1.0 : 20;
	float separation = 0.02;
	float base = 0.5;
	float scale = 0.0004;
	

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

float2 cloudinate(float3 position, float3 sun_direction, float height, float2 offset) {
	bool hit;
	float3 final_position;
	intersectPlane_float(float3(0, height, 0), float3(0, 1, 0), position, -sun_direction, hit, final_position);
	return hit ? (final_position.xz + unity_noise_randomValue(float2(height, 1123.321)) * 11232.231 + offset) : 0.0; 
}

float3 normalStrength(float3 val, float strength) {
	return float3(val.x * strength, lerp(1, val.y, saturate(strength)), val.z * strength);
}

float3 light(float2 deriv, float3 sun, float3 ray) {
	float3 normal = normalize(float3(deriv.x, 1, deriv.y));
	float d = clamp(dot(sun, normal) * 0.5 + 0.5, 0, 1);
	float s = pow(max(dot(sun, normalize(ray + abs(normal) * 0.4)), 0), 30);
	//float ss = pow(dot(reflect(ray, normal), sun) * 0.5 + 0.5, 20);
	float ss = pow(length(abs(normal).xz), 5);

	float strength = 1.0 * (1 - abs(sun.y) * 0.3);

	float ambient = clamp(-sun.y * 3.5 + 0.05, 0.01, 1);
	half3 ambientLighting = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

	//float t = dot(normalize(sun), abs(normal)) * 0.5 + 0.5;
	//return d + ss;
	//return float3(0,0,0);
	
	//return s;
	//return 	SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, my_point_clamp_sampler, -ray, 0).xyz;
	//return SampleSH9(SHCoefficients, normal);
	//return ambientLighting;
	//return lerp(d * strength, 1.0, s) * ambient;
	float3 amb = EvaluateAmbientProbe(normal);
	return lerp(amb * 0.5, 1, d + ss * 0.5) + s;
}


void sampleAllClouds_float(float3 ray_start, float3 ray_dir, float3 sun, bool shadowed, out float alpha, out float3 colour) {
	if (shadowed) {
		//ray_start = round(ray_start / 3) * 3;
	}

	float alpha1 = 1.0;
	float2 derivative1 = 0.0;
	doTheThing(cloudinate(ray_start, ray_dir, 3000, _baseOffset0), shadowed, alpha1, derivative1);
	alpha = alpha1;
	float3 light1 = light(derivative1, sun, ray_dir);

	float alpha2 = 1.0;
	float2 derivative2 = 0.0;
	doTheThing(cloudinate(ray_start, ray_dir, 5000, _baseOffset1), shadowed, alpha2, derivative2);
	alpha += alpha2;
	float3 light2 = light(derivative2, sun, ray_dir);

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