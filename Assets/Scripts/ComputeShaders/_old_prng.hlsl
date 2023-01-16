

float prng_SchechterBridson(uint seed, float min, float max)
{
    // Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
    seed ^= 2747636419u;
    seed *= 2654435769u;
    seed ^= seed >> 16;
    seed *= 2654435769u;
    seed ^= seed > 16;
    seed *= 2654435769u;
    
    float rgn = float(seed) / 4294967295.0; // 2^32-1
    return lerp(min, max, rgn);
}

float prng_FractSinDot(float2 seed, float min, float max)
{
    float randomno =  frac(sin(dot(seed, float2(12.9898, 78.233)))*43758.5453);
    return lerp(min, max, randomno);
}

/*
int s0 = 4120;
int s1 = 1300;
int s2 = 490;
int n = 10000;
float p_n = 0;
float frns_n = 0;
float prng_Itamaraca(float min, float max)
{
    p_n = abs(s2-s1+s1-s0);
    frns_n = abs(n - p_n*sqrt(3.9));

    s0 = s1;
    s1 = s2;
    s2 = frns_n;

    return lerp(min, max, frns_n);
}
*/
/*
static uint state = 0x4d595df4d0f33173;	// Or something seed-dependent
static uint multiplier = 6364136223846793005u;
static uint increment  = 1442695040888963407u;	// Or an arbitrary odd constant
static bool initialized = false;

float prng_PCG32float(float min, float max)
{
	if(!initialized)
	{
		//state = seed + increment;
		initialized = true;
	}
		
	uint x = state;
	unsigned count = (unsigned)(x >> 59);		// 59 = 64 - 5
	state = x * multiplier + increment;
	x ^= x >> 18;								// 18 = (64 - 27)/2
	return lerp(min, max, ldexp(x >> count | x << (-count & 31) ,-32));	// 27 = 32 - 5
}
*/