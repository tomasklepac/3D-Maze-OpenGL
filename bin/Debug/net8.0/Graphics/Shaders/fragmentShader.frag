#version 330 core

// === Vertex Outputs ===
in vec3 Normal;
in vec3 FragPos;
in vec2 vTexCoord;
in mat3 TBN;

// === Output Color ===
out vec4 FragColor;

// === Spotlight Parameters ===
uniform vec3 lightPos;        // Position of the spotlight
uniform vec3 lightDirection;  // Direction the spotlight is pointing
uniform float cutOff;         // Inner cutoff (cosine)
uniform float outerCutOff;    // Outer cutoff (cosine) for soft edge

// === Camera & Light Info ===
uniform vec3 viewPos;         // Camera position
uniform vec3 lightColor;      // White or colored light

// === PBR Textures ===
uniform sampler2D texture0;       // Albedo
uniform sampler2D normalMap;      // Normal map (in tangent space)
uniform sampler2D roughnessMap;   // Roughness (0 = smooth, 1 = rough)
uniform sampler2D metallicMap;    // Metallic (0 = dielectrics, 1 = metal)
uniform sampler2D aoMap;          // Ambient occlusion
uniform sampler2D heightMap;      // Heightmap (for parallax/offset mapping)

void main()
{
    // === Parallax Mapping ===
    float heightScale = 0.05;
    vec3 viewDirTangent = normalize(TBN * (viewPos - FragPos));
    float height = texture(heightMap, vTexCoord).r;
    vec2 parallaxOffset = viewDirTangent.xy * (height * heightScale);
    vec2 texCoords = vTexCoord - parallaxOffset;

    // === Normal Mapping ===
    vec3 sampledNormal = texture(normalMap, texCoords).rgb;
    sampledNormal = normalize(sampledNormal * 2.0 - 1.0); // [0,1] -> [-1,1]
    vec3 norm = normalize(TBN * sampledNormal);           // Tangent space to world space

    // === View vector ===
    vec3 viewDir = normalize(viewPos - FragPos);

    // === Ambient ===
    float ao = texture(aoMap, texCoords).r;
    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * lightColor * ao;

    // === Spotlight Lighting ===
    vec3 lightDir = normalize(lightPos - FragPos);
    float theta = dot(lightDir, normalize(-lightDirection)); // Angle between light and direction to fragment

    float epsilon = cutOff - outerCutOff;
    float spotIntensity = clamp((theta - outerCutOff) / epsilon, 0.0, 1.0); // Soft edge spotlight

    // === Diffuse ===
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor * spotIntensity;

    // === Specular ===
    vec3 reflectDir = reflect(-lightDir, norm);
    float roughness = texture(roughnessMap, texCoords).r;
    float metallic = texture(metallicMap, texCoords).r;
    float specularStrength = 1.0 - roughness;

    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    vec3 specular = specularStrength * spec * lightColor * spotIntensity;

    // === Final Composition ===
    vec4 texColor = texture(texture0, texCoords);
    vec3 baseColor = pow(texColor.rgb, vec3(1.5)); // Slight gamma boost
    vec3 litColor = ambient + diffuse + specular;

    // Mix base lighting and metallic reflection
    vec3 result = mix(litColor * baseColor, specular * lightColor, metallic);

    FragColor = vec4(result, 1.0);
}
