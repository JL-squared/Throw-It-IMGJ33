// https://www.shadertoy.com/view/XdXBRH
// https://iquilezles.org/articles/gradientnoise/
// The MIT License
// Copyright © 2017 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org

float2 hash( in int2 p )  // this hash is not production ready, please
{                        // replace this by something better

    // 2D -> 1D
    int2 n = p.x*int2(3,37) + p.y*int2(311,113);

    // 1D hash by Hugo Elias
	n = (n << 13) ^ n;
    n = n * (n * n * 15731 + 789221) + 1376312589;
    return -1.0+2.0*float2( n & int2(0x0fffffff, 0x0fffffff))/float(0x0fffffff);
}

// returns 3D value noise (in .x)  and its derivatives (in .yz)
float3 noised( float2 p )
{
    float2 i = floor( p );
    float2 f = frac( p );

    float2 u = f*f*f*(f*(f*6.0-15.0)+10.0);
    float2 du = 30.0*f*f*(f*(f-2.0)+1.0);
    
    float2 ga = hash( i + float2(0.0,0.0) );
    float2 gb = hash( i + float2(1.0,0.0) );
    float2 gc = hash( i + float2(0.0,1.0) );
    float2 gd = hash( i + float2(1.0,1.0) );
    
    float va = dot( ga, f - float2(0.0,0.0) );
    float vb = dot( gb, f - float2(1.0,0.0) );
    float vc = dot( gc, f - float2(0.0,1.0) );
    float vd = dot( gd, f - float2(1.0,1.0) );

    return float3( va + u.x*(vb-va) + u.y*(vc-va) + u.x*u.y*(va-vb-vc+vd),   // value
                 ga + u.x*(gb-ga) + u.y*(gc-ga) + u.x*u.y*(ga-gb-gc+gd) +  // derivatives
                 du * (u.yx*(va-vb-vc+vd) + float2(vb,vc) - va));
}
