// https://godotshaders.com/shader/ps1-post-processing/
void MyFunctionA_float(float2 frag, float3 coloured, float2 uv, float2 texelSize, out float3 tahini)
{
	uint3 rounded = uint3(round(coloured * 255.0));
	int depth = 3;

	const int pattern[] = {
		-4, +0, -3, +1, 
		+2, -2, +3, -1, 
		-3, +1, -4, +0, 
		+3, -1, +2, -2
	};
	
	int2 rounded2 = (frag / texelSize)%2.0;
	int x = round(frag.x) % 4;
	int y = round(frag.y) % 4;

	int dither = pattern[y * 4 + x];
	int3 temp = int3(rounded);
	
	temp.x += rounded2.x * 100;
	//temp += dither;
	temp = clamp(temp, 0, 255);
	rounded = uint3(temp);
	rounded = clamp(rounded, 0, 255);

	//rounded >>= (8 - depth);
	
	//tahini = float3(rounded) / float(1 << depth);
	tahini = rounded / 255.0;
}

void GetRenderScale_float(out float2 renderScale) {
	renderScale = _ScreenSize.xy / _ScreenParams.xy;
}
