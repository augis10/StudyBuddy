﻿using StudyBuddyShared.Network;
using StudyBuddyShared.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StudyBuddyShared.Utility
{
    public static class KarmaBadgeFetcher
    {
        public static List<KarmaBadge> KarmaBadges { get; set; }
        public static KarmaBadge CurrentBadge(int value)
        {
            KarmaBadges = (List<KarmaBadge>)KarmaBadges.OrderBy(karmaBadge => karmaBadge.StartValue);
            for (int i = 0; i < KarmaBadges.Count; i++)
            {
                if (i + 1 >= KarmaBadges.Count)
                {
                    return KarmaBadges[i];
                }

                if (KarmaBadges[i].StartValue <= value && KarmaBadges[i + 1].StartValue <= value)
                {
                    continue;
                }

                if (KarmaBadges[i].StartValue <= value)
                {
                    return KarmaBadges[i];
                }
                else if (KarmaBadges[i + 1].StartValue <= value)
                {
                    return KarmaBadges[i + 1];
                }
            }

            return null;
        }

        public static KarmaBadge NextBadge(int value)
        {
            KarmaBadge currentBadge = CurrentBadge(value);
            if (KarmaBadges.Count == KarmaBadges.IndexOf(currentBadge) + 1)
            {
                return KarmaBadges[KarmaBadges.Count];
            }

            return KarmaBadges[KarmaBadges.IndexOf(currentBadge) + 1];

        }
    }
}
