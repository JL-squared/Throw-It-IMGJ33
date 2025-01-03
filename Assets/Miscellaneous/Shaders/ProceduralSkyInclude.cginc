// literally copy pasted from https://www.shadertoy.com/view/llffzM
// and from https://www.shadertoy.com/view/Ml2cWG

const float pi = 3.14159265359;

const float zenithOffset = 0.0;
const float multiScatterPhase = 0.2;
const float density = 0.6;

const float anisotropicIntensity = 0.0; //Higher numbers result in more anisotropic scattering


#define smooth(x) x*x*(3.0-2.0*x)
#define zenithDensity(x) density / pow(max(x - zenithOffset, 0.35e-2), 0.75)

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

float3 getAtmosphericScattering(float2 p, float2 lp){		
	float3 skyColor = float3(0.39, 0.57, 1.0) * (1.0 + anisotropicIntensity); //Make sure one of the conponents is never 0.0
	float zenith = zenithDensity(p.y);
	float sunPointDistMult =  clamp(length(max(lp.y + multiScatterPhase - zenithOffset, 0.0)), 0.0, 1.0);
	
	float rayleighMult = getRayleigMultiplier(p, lp);
	
	float3 absorption = getSkyAbsorption(skyColor, zenith);
    float3 sunAbsorption = getSkyAbsorption(skyColor, zenithDensity(lp.y + multiScatterPhase));
	float3 sky = skyColor * zenith * rayleighMult;
	float3 sun = getSunPoint(p, lp) * absorption;
	float3 mie = getMie(p, lp) * sunAbsorption;
	
	float3 totalSky = lerp(sky * absorption, sky / (sky + 0.5), sunPointDistMult);
//         totalSky += sun + mie;
	     totalSky *= sunAbsorption * 0.5 + 0.5 * length(sunAbsorption);

    return totalSky;
}

float2 screen2world(float2 pos)
{
    return (pos - 0.5) * float2(2., 1.) * pi;
}

// A bit of conversion magic from https://learnopengl.com/PBR/IBL/Diffuse-irradiance
const float2 invAtan = float2(0.1591, 0.3183);
float2 sample_spherical_map(float3 v)
{
    float2 uv = float2(atan2(v.z, v.x), asin(v.y));
    uv *= invAtan;
    uv += 0.5;
    return uv;
}

// Calculate a procedural sky color based on a multitude of gradients
float3 sky(
    float3 normal,
    float3 sun
) {
	//return float3(1);
    float2 test_normal = sample_spherical_map(normal);
    float2 test_sun = sample_spherical_map(-sun);
    float3 color = getAtmosphericScattering(screen2world(test_normal), screen2world(test_sun)) * 0.5;
    return normal;
}

void MyFunctionA_float(float3 normal, float3 sun, out float3 colour)
{
	colour = sky(normal, float3(0, 1, 0));
}