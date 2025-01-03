// https://godotshaders.com/shader/ps1-post-processing/
void MyFunctionA_float(float2 frag, float3 coloured, out float3 tahini)
{
	uint3 rounded = uint3(round(coloured * 255.0));
	int depth = 3;

	const int pattern[] = {
		-4, +0, -3, +1, 
		+2, -2, +3, -1, 
		-3, +1, -4, +0, 
		+3, -1, +2, -2
	};
	
	int x = int(frag.x) % 4;
	int y = int(frag.y) % 4;

	int temp = pattern[y * 4 + x];
	if (all(rounded >= 4)) {
		rounded += temp;
	}
	rounded = clamp(rounded, 0, 255);

	//rounded >>= (8 - depth);
	
	//tahini = float3(rounded) / float(1 << depth);
	tahini = rounded / 255.0;
}