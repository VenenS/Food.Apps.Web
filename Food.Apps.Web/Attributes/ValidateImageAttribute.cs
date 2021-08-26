using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Food.Apps.Web.Attributes
{
    /// <summary>
    /// Проверяет загруженное изображение на валидность. Атрибут крепится к
    /// полю с типом IFormFile.
    /// </summary>
    public class ValidateImageAttribute : ValidationAttribute
    {
        /// <summary>
        /// Разрешенные mimetype изображений через запятую.
        /// </summary>
        public string AllowedMimetypes { get; set; }

        /// <summary>
        /// Макс. ширина изображения в пикселях.
        /// </summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// Макс. высота изображения в пикселях.
        /// </summary>
        public int MaxHeight { get; set; }

        /// <summary>
        /// Макс. размер файла изображения в байтах.
        /// </summary>
        public int MaxSizeBytes { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file == null)
                return ValidationResult.Success;

            // Проверка используется ли разрешенный mimetype.
            var mimes = !string.IsNullOrEmpty(AllowedMimetypes)
                ? AllowedMimetypes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : null;
            if (mimes != null && !mimes.Contains(file.ContentType?.ToLowerInvariant() ?? ""))
                return new ValidationResult("Формат изображения не поддерживается");

            // Проверка размера файла.
            if (MaxSizeBytes > 0 && file.Length > MaxSizeBytes) {
                var msg = $"Размер изображения {file.Length} байт превышает допустимый размер в {MaxSizeBytes} байт";
                return new ValidationResult(msg);
            }

            // Проверка макс. разрешения и читабельности файла.
            try {
                // FIXME: все равно возможно пронести изображение неразрешенного формата,
                // лишь бы формат поддерживался нетом.
                using (var stream = file.OpenReadStream())
                using (var img = Image.FromStream(stream)) {
                    if (MaxWidth > 0 && MaxHeight > 0) {
                        if (img.Width > MaxWidth || img.Height > MaxHeight) {
                            var msg = $"Размер изображения должен быть не больше {MaxWidth}x{MaxHeight}";
                            return new ValidationResult(msg);
                        }
                    } else if (MaxWidth > 0 && img.Width > MaxWidth) {
                        return new ValidationResult($"Ширина изображения должна быть не больше {MaxWidth} пикселей");
                    } else if (MaxHeight > 0 && img.Height > MaxHeight) {
                        return new ValidationResult($"Высота изображения должна быть не больше {MaxWidth} пикселей");
                    }
                }
            } catch (ArgumentException) {
                return new ValidationResult("Неизвестный формат изображения");
            }

            return ValidationResult.Success;
        }
    }
}
