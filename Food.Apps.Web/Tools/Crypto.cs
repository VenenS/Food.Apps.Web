using System;
using System.Security.Cryptography;
using ITWebNet.Food.Core;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site
{
    public static class Crypto
    {
        /// <summary>
        /// Конвертирует hex строку в массив byte[].
        /// </summary>
        public static byte[] Unhexify(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Input string length must be divisible by 2");
            var result = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length / 2; i++)
            {
                var c = hex.Substring(i * 2, 2);
                result[i] = byte.Parse(c, System.Globalization.NumberStyles.HexNumber);
            }
            return result;
        }

        /// <summary>
        /// Сериализует объект в json и шифрует полученный результат. Используется aes-ctr.
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ для шифрования (в формате hex, 32 символа)</param>
        /// <param name="data">Объект для сериализации и шифрования</param>
        public static string Encrypt<T>(string key, T data)
        {
            // FIXME: пропустить ключ через kdf вместо передачи конечного ключа
            // фиксированной длины/формата.

            // Желательно использовать aes-gcm, но таргетируемый .net не поддерживает
            // этот режим, поэтому городим альтернативный вариант на основе aes-ctr.
            var marshalled = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(marshalled);
            var iv = new byte[16];
            var output = new byte[iv.Length + bytes.Length];

            // Генерация IV.
            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(iv);
                Array.Copy(iv, 0, output, 0, iv.Length);
            }

            // Шифрование.
            using (var mode = new Aes128CounterMode(iv))
            {
                using (var xform = mode.CreateEncryptor(Unhexify(key), null))
                {
                    xform.TransformBlock(bytes, 0, bytes.Length, output, iv.Length);
                }
            }

            // Конвертация результата в base64.
            // TODO: подписать результат, оч. важно!
            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// Расшифровывает объект предварительно зашифрованный с помощью <see cref="Encrypt{T}(string, T)"/>.
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ для шифрования (в формате hex, 32 символа)</param>
        /// <param name="data">Строка для дешифровки и десериализации в объект</param>
        /// <exception cref="InvalidOperationException">
        /// Исключение бросаемое в случае ошибки дешифровки или десериализации данных.
        /// </exception>
        public static T Decrypt<T>(string key, string data)
        {
            var bytes = Convert.FromBase64String(data);
            var iv = new byte[16];

            if (bytes.Length < iv.Length + 1)
                throw new ArgumentException("Input blob is too short");

            var output = new byte[bytes.Length - iv.Length];
            Array.Copy(bytes, 0, iv, 0, iv.Length);

            try
            {
                using (var mode = new Aes128CounterMode(iv))
                {
                    using (var xform = mode.CreateDecryptor(Unhexify(key), null))
                    {
                        xform.TransformBlock(bytes, iv.Length, bytes.Length - iv.Length, output, 0);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to decrypt given blob", e);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(output));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while trying to deserialize data into an object", e);
            }
        }
    }
}