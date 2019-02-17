using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoyPngConverter
{
    internal class Sprite
    {
        private int hashcode;

        public Sprite()
        {
            Bytes = new List<byte>();
        }

        internal List<Byte> Bytes
        {
            get;set;
        }

        internal int HashCode
        {
            get
            {
                if (hashcode == 0)
                {
                    hashcode = this.GetHashCode();
                }
                return hashcode;
            }
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                Bytes.ForEach(b =>
                {
                    hash = hash * 23 + b.GetHashCode();
                });
                return hash;
            }
        }
    }
}
