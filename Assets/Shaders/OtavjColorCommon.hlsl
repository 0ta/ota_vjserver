float3 rgb2hsv(float3 rgb)
{
    float3 hsv;

    // RGB�̎O�̒l�ōő�̂���
    float maxValue = max(rgb.r, max(rgb.g, rgb.b));
    // RGB�̎O�̒l�ōŏ��̂���
    float minValue = min(rgb.r, min(rgb.g, rgb.b));
    // �ő�l�ƍŏ��l�̍�
    float delta = maxValue - minValue;
            
    // V�i���x�j
    // ��ԋ����F��V�l�ɂ���
    hsv.z = maxValue;
            
    // S�i�ʓx�j
    // �ő�l�ƍŏ��l�̍��𐳋K�����ċ��߂�
    if (maxValue != 0.0)
    {
        hsv.y = delta / maxValue;
    }
    else
    {
        hsv.y = 0.0;
    }
            
    // H�i�F���j
    // RGB�̂����ő�l�ƍŏ��l�̍����狁�߂�
    if (hsv.y > 0.0)
    {
        if (rgb.r == maxValue)
        {
            hsv.x = (rgb.g - rgb.b) / delta;
        }
        else if (rgb.g == maxValue)
        {
            hsv.x = 2 + (rgb.b - rgb.r) / delta;
        }
        else
        {
            hsv.x = 4 + (rgb.r - rgb.g) / delta;
        }
        hsv.x /= 6.0;
        if (hsv.x < 0)
        {
            hsv.x += 1.0;
        }
    }
    
    return hsv;
}
        
// HSV->RGB�ϊ�
float3 hsv2rgb(float3 hsv)
{
    float3 rgb;

    if (hsv.y == 0)
    {
        // S�i�ʓx�j��0�Ɠ������Ȃ�Ζ��F�������͊D�F
        rgb.r = rgb.g = rgb.b = hsv.z;
    }
    else
    {
        // �F��H�i�F���j�̈ʒu��S�i�ʓx�j�AV�i���x�j����RGB�l���Z�o����
        hsv.x *= 6.0;
        float i = floor(hsv.x);
        float f = hsv.x - i;
        float aa = hsv.z * (1 - hsv.y);
        float bb = hsv.z * (1 - (hsv.y * f));
        float cc = hsv.z * (1 - (hsv.y * (1 - f)));
        if (i < 1)
        {
            rgb.r = hsv.z;
            rgb.g = cc;
            rgb.b = aa;
        }
        else if (i < 2)
        {
            rgb.r = bb;
            rgb.g = hsv.z;
            rgb.b = aa;
        }
        else if (i < 3)
        {
            rgb.r = aa;
            rgb.g = hsv.z;
            rgb.b = cc;
        }
        else if (i < 4)
        {
            rgb.r = aa;
            rgb.g = bb;
            rgb.b = hsv.z;
        }
        else if (i < 5)
        {
            rgb.r = cc;
            rgb.g = aa;
            rgb.b = hsv.z;
        }
        else
        {
            rgb.r = hsv.z;
            rgb.g = aa;
            rgb.b = bb;
        }
    }
    return rgb;
}