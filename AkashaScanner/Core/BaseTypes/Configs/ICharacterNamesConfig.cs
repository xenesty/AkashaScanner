﻿using System;
using System.Collections.Generic;

namespace AkashaScanner.Core
{
    public interface ICharacterNamesConfig
    {
        Dictionary<string, string> CharacterNameOverrides { get; set; }
    }
}
