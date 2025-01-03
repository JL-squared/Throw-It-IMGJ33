// literally copy pasted from https://www.shadertoy.com/view/llffzM
// and from https://www.shadertoy.com/view/Ml2cWG

#define pi 3.14159265359

//Higher numbers result in more anisotropic scattering

float zenithDensity(float x, float density, float zenithOffset) {
	return density / pow(max(x - zenithOffset, 0.35e-2), 0.75);
}

float greatCircleDist(float2 p, float2 lp)
{
    float phi_1 = p.y;
    float phi_2 = lp.y;
    float delta_lambda = p.x-lp.x;
    return acos(sin(phi_1)*sin(phi_2) + cos(phi_1)*cos(phi_2)*cos(delta_lambda));
}

float3 getSkyAbsorption(float3 x, float y){
	
	float3 absorption = x * -y;
	absorption = exp2(absorption) * 2.0;
	return absorption;
}

float getSunPoint(float2 p, float2 lp){
    float dist = greatCircleDist(p, lp)/pi*2.;
	return smoothstep(0.03, 0.026, dist) * 50.0;
}

float getRayleigMultiplier(float2 p, float2 lp)
{
    float dist = greatCircleDist(p, lp)/pi*5.;
	return 1.0 + pow(1.0 - clamp(dist, 0.0, 1.0), 2.0) * pi * 0.5;
}

float getMie(float2 p, float2 lp){
    float dist = greatCircleDist(p, lp)/pi*2.;
	float disk = clamp(1.0 - pow(dist, 0.1), 0.0, 1.0);
	
	return disk*disk*(3.0 - 2.0 * disk) * 2.0 * pi;
}

float3 getAtmosphericScattering(float2 p, float2 lp, float multiScatterPhase, float density, float zenithOffset, float anisotropicIntensity, float3 skyColorParams){		
	float3 skyColor = skyColorParams * (1.0 + anisotropicIntensity); //Make sure one of the conponents is never 0.0
	float zenith = zenithDensity(p.y, density, zenithOffset);
	float sunPointDistMult =  clamp(length(max(lp.y + multiScatterPhase - zenithOffset, 0.0)), 0.0, 1.0);
	
	float rayleighMult = getRayleigMultiplier(p, lp);
	
	float3 absorption = getSkyAbsorption(skyColor, zenith);
    float3 sunAbsorption = getSkyAbsorption(skyColor, zenithDensity(lp.y + multiScatterPhase, density, zenithOffset));
	float3 sky = skyColor * zenith * rayleighMult;
	float3 sun = getSunPoint(p, lp) * absorption;
	float3 mie = getMie(p, lp) * sunAbsorption;
	
	float3 totalSky = lerp(sky * absorption, sky / (sky + 0.5), sunPointDistMult);
         totalSky += sun;
	     totalSky *= sunAbsorption * 0.5 + 0.5 * length(sunAbsorption);
	return totalSky;
}

float2 screen2world(float2 pos)
{
    return (pos - 0.5) * float2(2., 1.) * pi;
}

// A bit of conversion magic from https://learnopengl.com/PBR/IBL/Diffuse-irradiance
#define invAtanTest float2(0.1591, 0.3183);
float2 sample_spherical_map(float3 v)
{
    float2 uv = float2(atan2(v.z, v.x), asin(v.y * 0.999));
    uv *= invAtanTest;
    uv += 0.5;
    return uv;
}

// Calculate a procedural sky color based on a multitude of gradients
float3 sky(
    float3 normal,
    float3 sun,
	float multiScatterPhase,
	float density,
	float zenithOffset,
	float anisotropicIntensity,
	float3 skyColorParams
) {
    float2 test_normal = sample_spherical_map(normal);
    float2 test_sun = sample_spherical_map(-sun);
    float3 color = getAtmosphericScattering(screen2world(test_normal), screen2world(test_sun), multiScatterPhase, density, zenithOffset, anisotropicIntensity, skyColorParams) * 0.5;
    return color;
}

void MyFunctionA_float(float3 normal, float3 sun, float multiScatterPhase, float density, float zenithOffset, float anisotropicIntensity, float3 skyColorParams, out float3 colour)
{
	colour = sky(normal, sun, multiScatterPhase, density, zenithOffset, anisotropicIntensity, skyColorParams);
}