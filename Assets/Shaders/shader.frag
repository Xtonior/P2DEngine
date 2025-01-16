#version 330 core

in vec2 texCoord;

out vec4 FragColor;

// Uniforms
uniform vec2 u_resolution;
uniform vec2 u_cameraPos;
uniform vec2 u_cameraDir;

uniform vec2 u_rayStart;
uniform vec2 u_rayEnd;

uniform sampler2D mapTexture;

void main() 
{
    vec2 uv = texCoord;
    float pixelThickness = 1.0 / u_resolution.y;

    float px = u_cameraPos.x / u_resolution.x; 
    float py = u_cameraPos.y / u_resolution.y; 

    float mapTex = texture(mapTexture, uv).r;
    vec3 col = vec3(mapTex);

    float playerThickness = 5.0;

    if (abs(uv.x - px) < pixelThickness * playerThickness * 0.5 &&
        abs(uv.y - py) < pixelThickness * playerThickness * 0.5) 
    {
        col = vec3(1.0, 0.7, 0.3);
    }

    float rayThickness = 2.0;

    float rx1 = u_rayStart.x / u_resolution.x; 
    float ry1 = u_rayStart.y / u_resolution.y; 

    float rx2 = u_rayEnd.x / u_resolution.x; 
    float ry2 = u_rayEnd.y / u_resolution.y; 

    // Calculate slope and intercept of the line
    float m = (ry2 - ry1) / (rx2 - rx1); // slope
    float b = ry1 - m * rx1; // y-intercept
    
    // Ensure rx1 is the smaller value and rx2 is the larger value
    float startX = min(rx1, rx2);
    float endX = max(rx1, rx2);
    
    // Loop from startX to endX
    for (float rx = startX; rx <= endX; rx += pixelThickness)
    {
        float ry = m * rx + b; // calculate the y position for this x
    
        if (abs(uv.x - rx) < pixelThickness * rayThickness * 0.5 && abs(uv.y - ry) < pixelThickness * rayThickness * 0.5)
        {
            col = vec3(0.5, 0.1, 0.3); // Set color when in range
        }
    }   

    FragColor = vec4(col, 1.0);
}