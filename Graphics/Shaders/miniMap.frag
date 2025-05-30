#version 330 core

// === Input from vertex shader ===
in vec3 vColor;

// === Output fragment color ===
out vec4 FragColor;

// === Circular mask uniforms ===
uniform vec2 uCenter;   // Center of the circular mask (in screen space)
uniform float uRadius;  // Radius of the visible area

void main()
{
    // Discard fragments outside the circular minimap radius
    if (distance(gl_FragCoord.xy, uCenter) > uRadius)
        discard;

    // Output the tile or player color
    FragColor = vec4(vColor, 1.0);
}
