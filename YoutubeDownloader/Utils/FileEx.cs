using System.IO;

namespace YoutubeDownloader.Utils
{
    /// <summary>
    /// 提供扩展方法以处理路径相关操作的静态类。
    /// </summary>
    internal static class PathEx
    {
        /// <summary>
        /// 确保返回一个在指定目录下唯一且不存在的文件路径。如果原始路径对应的文件已存在，
        /// 则在其名称后附加递增数字，直到找到一个不存在的文件名或达到最大重试次数为止。
        /// 默认最大重试次数为100次。
        /// </summary>
        /// <param name="baseFilePath">要检查并确保其唯一性的基础文件路径。</param>
        /// <param name="maxRetries">尝试生成唯一文件名的最大次数，默认值为100。</param>
        /// <returns>返回一个在指定目录下唯一且不存在的文件路径，若超过最大重试次数仍未找到，则返回原始路径。</returns>
        public static string EnsureUniquePath(string baseFilePath, int maxRetries = 100)
        {
            if (!File.Exists(baseFilePath))
                return baseFilePath;

            // 获取基础文件路径的目录部分
            var baseDirPath = Path.GetDirectoryName(baseFilePath);

            // 分离基础文件名和扩展名1
            var baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            var baseFileExtension = Path.GetExtension(baseFilePath);

            // 尝试生成唯一的文件名
            for (var i = 1; i <= maxRetries; i++)
            {
                // 构造带有编号的新文件名
                var fileName = $"{baseFileNameWithoutExtension} ({i}){baseFileExtension}";

                // 根据目录信息组合出完整新路径
                var filePath = !string.IsNullOrWhiteSpace(baseDirPath)
                    ? Path.Combine(baseDirPath, fileName)
                    : fileName;

                // 检查新路径对应的文件是否存在
                if (!File.Exists(filePath))
                    return filePath;
            }

            // 若超过最大重试次数，仍返回原始路径
            return baseFilePath;
        }
    }
}