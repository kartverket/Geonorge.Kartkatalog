using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public interface ArticleIndexer
    {
        void RunIndexing();
        void RunIndexingOn(string uuid);
        void RunReIndexing();
    }
}
