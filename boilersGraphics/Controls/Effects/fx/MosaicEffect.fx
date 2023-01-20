sampler2D input : register(s0);
float width : register(c0);
float height : register(c1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float x = 30.0;
    float y = 30.0;
    float l = 0.0;
    if (width > height)
    {
        x = x * width / height;
        l = 0.5 / y;
    }
    else
    {
        y = y * height / width;
        l = 0.5 / x;
    }
    float2 uv2 = float2(floor(uv.x * x) / x, floor(uv.y * y) / y);
    return tex2D(input, uv2);
}