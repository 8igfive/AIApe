using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;

namespace Buaa.AIBot.Repository
{
    public interface ITagRepostory
    {
        Task<IReadOnlyDictionary<string, int>> SelectAllTagsAsync();
        Task<IReadOnlyDictionary<TagCategory, IReadOnlyDictionary<int, string>>> SelectAllTagsCategorysAsync();
        Task<IReadOnlyDictionary<int, TagCategory>> SelectTagIndexAsync();

        /// <summary>
        /// Select a tag by id.
        /// </summary>
        /// <param name="tagId">tid</param>
        /// <returns>a TagInfo object if exist, or null. </returns>
        Task<TagInfo> SelectTagByIdAsync(int tagId);

        /// <summary>
        /// Insert a new tag. TagId will be generated automatically.
        /// </summary>
        /// <exception cref="ArgumentNullException">Name or Desc is null.</exception>
        /// <exception cref="TagNameTooLongException">Name is longger than <see cref="Constants.TagNameMaxLength"/>.</exception>
        /// <exception cref="TagNameHasExistException">There is already a tag has the same Name. </exception>
        /// <param name="tag">new tag to store. </param>
        /// <returns>tid</returns>
        Task<int> InsertTagAsync(TagInfo tag);

        /// <summary>
        /// Update the tag with tid=<paramref name="tag"/>.TagId.
        /// </summary>
        /// <exception cref="TagNotExistException">No tag has tid=<paramref name="tag"/>.TagId.</exception>
        /// <exception cref="TagNameTooLongException">Name is longger than <see cref="Constants.TagNameMaxLength"/>.</exception>
        /// <exception cref="TagNameHasExistException">There is already a tag has the same Name. </exception>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task UpdateTagAsync(TagInfo tag);

        /// <summary>
        /// Make sure no tag whose Id is <paramref name="tagId"/>. (no operation if it has already not exist).
        /// </summary>
        /// <param name="tagId">tid</param>
        /// <returns></returns>
        Task DeleteTagAsync(int tagId);
    }
}
