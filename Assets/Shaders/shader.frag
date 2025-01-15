#version 330 core

in vec2 texCoord;

out vec4 FragColor;

uniform float screenX;
uniform float sliceStart;
uniform float sliceEnd;
uniform vec3 wallColor;

void main() 
{
    if (texCoord.x >= sliceStart && texCoord.x <= sliceEnd) 
    {
        // .x * texCoord.y, wallColor.y * texCoord.y, wallColor.z * texCoord.y
        //  && texCoord.x == screenX
        FragColor = vec4(wallColor, 1.0);
    } 
    else 
    {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}