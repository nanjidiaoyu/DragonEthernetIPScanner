using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP
{
    public class EPath
    {
        private ushort _classId;
        private ushort _objectId;
        private ushort _attributeId;
        private ushort _size;
        public EPath(ushort classId)
        {
            _classId = classId;
            _size = 1;
        }
        public EPath(ushort classId, ushort objectId)
        {
            _classId = classId;
            _objectId = objectId;
            _size = 2;
        }
        public EPath(ushort classId, ushort objectId, ushort attributeId)
        {
            _classId = classId;
            _objectId = objectId;
            _attributeId = attributeId;
            _size = 3;
        }

        public List<byte> PackPaddedPath(bool use_8_bit_path_segments = false)
        {
            if (use_8_bit_path_segments)
            {
                DragonBuffer dragonBuffer = new DragonBuffer();
                var classSegment = (byte)EPathSegmentTypes.CLASS_8_BITS;
                dragonBuffer.Write(classSegment);
                dragonBuffer.Write(this._classId);
                if (_size > 1)
                {
                    var instanceSegment = (byte)EPathSegmentTypes.INSTANCE_8_BITS;
                    dragonBuffer.Write(instanceSegment);
                    dragonBuffer.Write(this._objectId);
                    if (_size > 2)
                    {
                        var attributeSegment = (byte)(EPathSegmentTypes.ATTRIBUTE_8_BITS);
                        dragonBuffer.Write(attributeSegment);
                        dragonBuffer.Write(this._attributeId);
                    }
                }
                return dragonBuffer.Data();
            }
            else
            {
                DragonBuffer dragonBuffer = new DragonBuffer();
                var classSegment = (ushort)EPathSegmentTypes.CLASS_16_BITS;
                dragonBuffer.Write(classSegment);
                dragonBuffer.Write(this._classId);
                if (_size > 1)
                {
                    var instanceSegment = (ushort)EPathSegmentTypes.INSTANCE_16_BITS;
                    dragonBuffer.Write(instanceSegment);
                    dragonBuffer.Write(this._objectId);
                    if (_size > 2)
                    {
                        var attributeSegment = (ushort)(EPathSegmentTypes.ATTRIBUTE_16_BITS);
                        dragonBuffer.Write(attributeSegment);
                        dragonBuffer.Write(this._attributeId);
                    }
                }
                return dragonBuffer.Data();
            }
        }

        public bool ExpandPaddedPath(List<byte> data)
        {
            DragonBuffer dragonBuffer = new DragonBuffer(data);
            this._classId = 0;
            this._objectId = 0;
            this._attributeId = 0;
            this._size = 0;
            for (int i = 0; i < data.Count && !dragonBuffer.empty(); ++i)
            {
                EPathSegmentTypes segmentType;
                byte ignore = 0;
                byte byteData;
                ushort word;
                segmentType = (EPathSegmentTypes)dragonBuffer.ReadUshort();
                switch (segmentType)
                {
                    case EPathSegmentTypes.CLASS_8_BITS:
                        byteData = dragonBuffer.ReadByte();
                        _classId = byteData;
                        break;
                    case EPathSegmentTypes.CLASS_16_BITS:
                        ignore = dragonBuffer.ReadByte();
                        word = dragonBuffer.ReadUshort();
                        _classId = word;
                        break;
                    case EPathSegmentTypes.INSTANCE_8_BITS:
                        byteData = dragonBuffer.ReadByte();
                        _objectId = byteData;
                        break;
                    case EPathSegmentTypes.INSTANCE_16_BITS:
                        ignore = dragonBuffer.ReadByte();
                        word = dragonBuffer.ReadUshort();
                        _objectId = word;
                        break;
                    case EPathSegmentTypes.ATTRIBUTE_8_BITS:
                        byteData = dragonBuffer.ReadByte();
                        _attributeId = byteData;
                        break;
                    case EPathSegmentTypes.ATTRIBUTE_16_BITS:
                        ignore = dragonBuffer.ReadByte();
                        word = dragonBuffer.ReadUshort();
                        _attributeId = word;
                        break;
                    default:
                        return false;
                }
            }

            if (!dragonBuffer.isValid())
            {
                return false;
            }

            if (_classId > 0)
            {
                _size++;

                if (_objectId > 0)
                {
                    _size++;

                    if (_attributeId > 0)
                    {
                        _size++;
                    }
                }
            }
            return true;
        }

        public ushort GetClassId()
        {
            return _classId;
        }
        public ushort GetObjectId()
        {
            return _objectId;
        }
        public ushort GetAttributeId()
        {
            return _attributeId;
        }
        public byte GetSizeInWords(bool use_8_bit_path_segments = false)
        {
            if (use_8_bit_path_segments)
            {
                return (byte)_size;
            }
            else
            {
                return (byte)(_size * 2);
            }
        }

        public override string ToString()
        {
            string msg = "[classId=" + _classId;
            if (_size > 1)
            {
                msg += " objectId=" + _objectId;
                if (_size > 2)
                {
                    msg += " attributeId=" + _attributeId;
                }
            }
            msg += "]";
            return msg;
        }

    }
}
