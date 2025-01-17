#version 330 core

#define M_PI 3.1415926535897932384626433832795
#define M_2PI 2 * 3.1415926535897932384626433832795

in vec2 texCoord;

out vec4 FragColor;

// Uniforms
uniform vec2 u_resolution;
uniform vec2 u_cameraPos;
uniform vec2 u_cameraDir;

uniform vec2 u_rayStart;

uniform sampler2D mapTexture;

float renderDist = 10.0f;
float mapTex;

vec3 raycast(vec2 origin, vec2 direction, sampler2D map, float maxDistance, float stepSize) 
{
    vec2 pos = origin;
    vec2 size = vec2(8, 8);

    for (float dist = 0.0; dist < maxDistance; dist += stepSize) 
    {
        vec2 uv = pos / size;

        if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) 
        {
            break;
        }

        float value = texture(map, uv).r;

        if (value > 0.5) 
        {
            return vec3(pos, 1.0);
        }

        pos += direction * stepSize;
    }

    return vec3(-1.0);
}

vec3 getCol3D(vec2 ro, vec2 rd, out float dist)
{
    vec3 r = raycast(ro, rd, mapTexture, renderDist, 0.1);
    if (r.z < 0.0) return vec3(0.0);

    vec2 hitPos = r.xy;
    float distance = length(hitPos - ro);

    if (hitPos.x > -1.0) 
    {
        dist = distance;
        return vec3(1.0);
    } 
    
    return vec3(0.0);
}

vec3 getVerticalLine(vec3 col, vec2 uv, float x, float y, float h, float pixel_thickness)
{
    if (distance(uv.x, x) < pixel_thickness / u_resolution.x &&
        distance(uv.y, y) < pixel_thickness / u_resolution.y * h)
    {
        return vec3(col);
    }

    return vec3(0.0);
}

vec3 getCol(vec2 uv)
{
    vec3 finalColor = vec3(0.0);

    float sliceHeight = 1.0 / u_resolution.y;
    float sliceY = (uv.y) * sliceHeight;

    for (int a = 0; a < u_resolution.x; a++) 
    {
        float ra = (a - 30.0) * (60.0 / 60.0);
        if (ra < 0.0) 
        {
            ra = M_2PI;
        }
        if (ra > M_2PI)
        {
            ra = 0.0;
        }

        float d;

        vec2 dir = vec2(cos(ra), sin(ra));
        vec3 col = getCol3D(u_rayStart, dir, d);

        float h = u_resolution.y / d * 0.000001;
        float brightness = 1.0 - d / renderDist;

        finalColor = getVerticalLine(col * brightness, uv, uv.x, 0.5, 300, h);
    }

    return finalColor;
}

void main() 
{
    vec2 uv = gl_FragCoord.xy / u_resolution.xy;
    
    mapTex = texture(mapTexture, uv).r;

    vec3 col;
    col = getCol(uv);
    FragColor = vec4(col, 1.0);
}