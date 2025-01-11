float invlerp(float a, float b, float c) {
	return (c - a) / (b - a);
}

float3 invlerp(float3 a, float3 b, float3 c) {
	return (c - a) / (b - a);
}

// https://godotshaders.com/shader/ps1-post-processing/
void MyFunctionA_float(float2 frag, float depth1, float multiplier, float tightness, float3 coloured, out float3 tahini)
{

	int depth = int(depth1);

	const int pattern[] = {
		-4, +0, -3, +1, 
		+2, -2, +3, -1, 
		-3, +1, -4, +0, 
		+3, -1, +2, -2
	};
	
	int x = round(frag.x) % 4;
	int y = round(frag.y) % 4;

	float dither = pattern[y * 4 + x];

	uint3 rounded2 = uint3(round(coloured * 255.0));
	float3 occured = (float3(rounded2 >> (8 - depth)) / float(1 << depth));

	float3 dist = coloured - occured;
	float3 diffs = (dist * multiplier - 0.5) * 0.5 + 0.5;

	float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (round(frag.x) % 4) * 4 + round(frag.y) % 4;

	float3 offset = 0.0;
	float3 val2 = clamp(invlerp(tightness, 1.0, abs(diffs)), 0, 1);
	float3 temp2 = val2 * 2.0f - DITHER_THRESHOLDS[index];
	offset = clamp(temp2, 0.0, 1.0);	
	uint3 rounded = uint3(round((coloured + offset / 255.0) * 255.0));
	int3 val = int3(rounded);
	val = max(val, 0);
	rounded = uint3(val);
	rounded = clamp(rounded, 0, 255);

	rounded >>= (8 - depth);
	tahini = float3(rounded) / float(1 << depth);	
	//tahini.x = 0;
	//tahini.y = 0;
	//tahini.z = 0;
	//tahini += offset / 255.0;
	//tahini += dither * 0.4;
}

void GetRenderScale_float(out float2 renderScale) {
	renderScale = _ScreenSize.xy / _ScreenParams.xy;
}

