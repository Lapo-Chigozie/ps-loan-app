using LapoLoanWebApi.ModelDto.PagenationFilterModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LapoLoanWebApi.ModelDto.PagenationHelper
{
    public class PagenationsHelper
    {
        private List<PageMode> PageModes = new List<PageMode>();
        private PagenationResponds Pagenation = new PagenationResponds();
        private async Task<PagenationResponds> GetPagenationNodes<T>(List<T> Data, PagenationFilterDto pagenationFilter)
        {
            var PagenationRespond = new PagenationResponds();
            bool IsStatus = false;
            try
            {
                var Loanheaders = Data;

                var TotalData = Loanheaders.Count;

                var pageCal = 6 * pagenationFilter.pageDataSize;

                long countPage = 0;

                long StartPage = 1;

                long countRows = 0;
                long i = 0;

                for (var a = 0; a < Data.Count; a++)
                {
                    countRows += 1;
                    i += 1;

                    var Ishoop = (TotalData - i) <= (TotalData - pagenationFilter.pageDataSize);

                    var hoop0 = (TotalData - pagenationFilter.pageDataSize);

                    var hoop1 = (TotalData - i);

                    if ((TotalData - i) < pagenationFilter.pageDataSize  && IsStatus == false)
                    {
                        if (TotalData > pagenationFilter.pageDataSize)
                        {
                            if(countRows == pagenationFilter.pageDataSize)
                            {
                                countPage += 1;

                                if (countPage == (int)pagenationFilter.SelectedNumber)
                                {
                                    PagenationRespond.MinSelectedNumber = StartPage;
                                    PagenationRespond.MaxSelectedNumber = i;
                                    PagenationRespond.PageSize = pagenationFilter.pageDataSize;
                                    PagenationRespond.ActivePagenation = countPage;
                                    PagenationRespond.StatusSearch = "";
                                    PagenationRespond.Search = "";
                                    PagenationRespond.TotalData = TotalData;
                                    PagenationRespond.Data = TotalData;
                                    PagenationRespond.PageModes = null;
                                    PagenationRespond.SkipLastData = (StartPage - 1);
                                    PagenationRespond.TakeData = pagenationFilter.pageDataSize;

                                    PageModes.Add(new PageMode() { IsSelected = pagenationFilter.IsSelected, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = i, SkipLastData = (StartPage - 1), TakeData = pagenationFilter.pageDataSize });
                                }
                                else
                                {
                                    PageModes.Add(new PageMode() { IsSelected = false, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = i, SkipLastData = (StartPage - 1), TakeData = pagenationFilter.pageDataSize });
                                }

                                countRows = 0;
                                StartPage = (long)(i + 1);
                            }
                            else if (Ishoop)
                            {
                                countPage += 1;

                                if (countPage == (int)pagenationFilter.SelectedNumber)
                                {
                                    PagenationRespond.MinSelectedNumber = StartPage;
                                    PagenationRespond.MaxSelectedNumber = i;
                                    PagenationRespond.PageSize = pagenationFilter.pageDataSize;
                                    PagenationRespond.ActivePagenation = countPage;
                                    PagenationRespond.StatusSearch = "";
                                    PagenationRespond.Search = "";
                                    PagenationRespond.TotalData = TotalData;
                                    PagenationRespond.Data = TotalData;
                                    PagenationRespond.PageModes = null;
                                    PagenationRespond.SkipLastData = (StartPage - 1);
                                    PagenationRespond.TakeData = (TotalData - i);


                                    PageModes.Add(new PageMode() { IsSelected = pagenationFilter.IsSelected, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = (i - 1) + ((TotalData - pagenationFilter.pageDataSize)), SkipLastData = (StartPage - 1), TakeData = (TotalData - pagenationFilter.pageDataSize) });

                                    IsStatus = true;

                                }
                                else
                                {
                                    PageModes.Add(new PageMode() { IsSelected = false, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = (i - 1) + ((TotalData - pagenationFilter.pageDataSize)), SkipLastData = (StartPage - 1), TakeData = (TotalData - pagenationFilter.pageDataSize) });

                                    IsStatus = true;
                                }

                                countRows = 0;
                                StartPage = (long)(i + 1);
                            }
                        }
                        else
                        {

                            if (countRows == pagenationFilter.pageDataSize)
                            {
                                countPage += 1;

                                if (countPage == (int)pagenationFilter.SelectedNumber)
                                {
                                    PagenationRespond.MinSelectedNumber = StartPage;
                                    PagenationRespond.MaxSelectedNumber = i;
                                    PagenationRespond.PageSize = pagenationFilter.pageDataSize;
                                    PagenationRespond.ActivePagenation = countPage;
                                    PagenationRespond.StatusSearch = "";
                                    PagenationRespond.Search = "";
                                    PagenationRespond.TotalData = TotalData;
                                    PagenationRespond.Data = TotalData;
                                    PagenationRespond.PageModes = null;
                                    PagenationRespond.SkipLastData = (StartPage - 1);
                                    PagenationRespond.TakeData = TotalData;

                                    PageModes.Add(new PageMode() { IsSelected = pagenationFilter.IsSelected, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = i, SkipLastData = (StartPage - 1), TakeData = TotalData });
                                }
                                else
                                {
                                    PageModes.Add(new PageMode() { IsSelected = false, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = i, SkipLastData = (StartPage - 1), TakeData = TotalData });
                                }

                                IsStatus = true;
                                countRows = 0;
                                StartPage = (long)(i + 1);
                            }
                            else /*if (Ishoop)*/
                            {
                                countPage += 1;

                                if (countPage == (int)pagenationFilter.SelectedNumber)
                                {
                                    PagenationRespond.MinSelectedNumber = StartPage;
                                    PagenationRespond.MaxSelectedNumber = i;
                                    PagenationRespond.PageSize = pagenationFilter.pageDataSize;
                                    PagenationRespond.ActivePagenation = countPage;
                                    PagenationRespond.StatusSearch = "";
                                    PagenationRespond.Search = "";
                                    PagenationRespond.TotalData = TotalData;
                                    PagenationRespond.Data = TotalData;
                                    PagenationRespond.PageModes = null;
                                    PagenationRespond.SkipLastData = (StartPage - 1);
                                    PagenationRespond.TakeData = (TotalData);

                                    PageModes.Add(new PageMode() { IsSelected = pagenationFilter.IsSelected, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = (i - 1) + ((TotalData - pagenationFilter.pageDataSize)), SkipLastData = (StartPage - 1), TakeData = (TotalData) });

                                    IsStatus = true;
                                }
                                else
                                {
                                    PageModes.Add(new PageMode() { IsSelected = false, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = (i - 1) + ((TotalData - pagenationFilter.pageDataSize)), SkipLastData = (StartPage - 1), TakeData = (TotalData) });
                                }

                                IsStatus = true;
                                countRows = 0;
                                StartPage = (long)(i + 1);
                            }
                        }
                    }
                    else
                    {
                        if (countRows == pagenationFilter.pageDataSize)
                        {
                            countPage += 1;

                            if (countPage == (int)pagenationFilter.SelectedNumber)
                            {
                                PagenationRespond.MinSelectedNumber = StartPage;
                                PagenationRespond.MaxSelectedNumber = i;
                                PagenationRespond.PageSize = pagenationFilter.pageDataSize;
                                PagenationRespond.ActivePagenation = countPage;
                                PagenationRespond.StatusSearch = "";
                                PagenationRespond.Search = "";
                                PagenationRespond.TotalData = TotalData;
                                PagenationRespond.Data = TotalData;
                                PagenationRespond.PageModes = null;
                                PagenationRespond.SkipLastData = (StartPage - 1);
                                PagenationRespond.TakeData = pagenationFilter.pageDataSize;

                                PageModes.Add(new PageMode() { IsSelected = pagenationFilter.IsSelected, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = i, SkipLastData = (StartPage - 1), TakeData = pagenationFilter.pageDataSize });

                            }
                            else
                            {
                                PageModes.Add(new PageMode() { IsSelected = false, TotalData = TotalData, SelectedNumber = countPage, StartPage = StartPage, EndPage = i, SkipLastData = (StartPage - 1), TakeData = pagenationFilter.pageDataSize });
                            }

                            countRows = 0;
                            StartPage = (long)(i + 1);
                        }

                        //else if(pagenationFilter.pageDataSize == 0)
                        //{

                        //}
                    }
                }

                if(PageModes.Count > 0)
                {
                    int Page1 = PageModes.IndexOf(PageModes.Single(i => i.IsSelected));

                    int Page2 = FindIndex(PageModes, x => x.IsSelected == true);

                    if (PageModes.Count <= 6)
                    {
                        PagenationRespond.LoopPageModes = PageModes.Skip(Page1 - 1).ToList().Take(PageModes.Count).ToList();
                    }
                    else
                    {
                        PagenationRespond.LoopPageModes = PageModes.Skip(Page1 - 1).ToList().Take(6).ToList();
                    }

                    if((Page1 - 1) < 1)
                    {
                        PagenationRespond.HasPreviousPagenation = false;
                        PagenationRespond.LastSelectedNumber = "";
                    }
                    else
                    {
                        PagenationRespond.HasPreviousPagenation = true;
                        PagenationRespond.LastSelectedNumber = (Page1 - 1).ToString().Replace("-", "");
                    }

                    //loopPageModes

                    if ((Page1 - 1) < (PageModes.Count - 1) && (Page1 - 1) > 0)
                    {
                        PagenationRespond.HasNextPagenation = true;

                        try
                        {
                            PagenationRespond.NextSelectedNumber = (PagenationRespond.LoopPageModes[PagenationRespond.LoopPageModes.Count - 1].SelectedNumber + 1).ToString();
                        }
                        catch(Exception ex)
                        {
                            PagenationRespond.NextSelectedNumber = "0";
                        }

                        PagenationRespond.TotalSelectedNumber = (PageModes.Count - 1).ToString().Replace("-", "");
                    }
                    else
                    {
                        PagenationRespond.HasNextPagenation = false;
                        PagenationRespond.TotalSelectedNumber = "";
                    }

                    PagenationRespond.PageModes = PageModes;
                }
                else
                {
                    PagenationRespond.PageModes = new List<PageMode>();
                    PagenationRespond.LoopPageModes = new List<PageMode>();
                }

                return PagenationRespond;
            }
            catch (Exception ex)
            {
                return PagenationRespond;
            }
        }

        public async Task<PagenationResponds> GetPagenation<T>(List<T> Data, PagenationFilterDto pagenationFilter)
        {
            try
            {
                if(pagenationFilter.pageDataSize == 0)
                {
                    pagenationFilter.pageDataSize = Data.Count;
                }   

                // var PageData = newLoanAppList.Skip((int)Pagenation.SkipLastData).Take((int)Pagenation.TakeData).ToList();

                // Pagenation.UnderPageCount = Pagenation.PageModes.Take(6).ToList().Count;

                // Pagenation.StatusSearch = pagenationFilter.status;
                // Pagenation.Search = pagenationFilter.searchText;
                // Pagenation.Data = PageData;

                //var ggs = Data.Count * 1000;

                //if (Data.Count > 0)
                //{
                //    for (var i = 0; i <= (ggs); i++)
                //    {
                //        Data.Add(Data[i]);
                //    }
                //}

                Pagenation = await new PagenationsHelper().GetPagenationNodes<T>(Data, pagenationFilter);

                var PageData = Data.Skip((int)Pagenation.SkipLastData).Take((int)Pagenation.TakeData).ToList();

                if (pagenationFilter != null && pagenationFilter.PageNext != null && pagenationFilter.PageNext.HasBar)
                {
                    PageData = Data.Skip((int)pagenationFilter.PageNext.SkipCount).Take((int)pagenationFilter.PageNext.TakeCount).ToList();
                }

                Pagenation.UnderPageCount = await this.GetUnderPageCount<PageMode>(Pagenation.PageModes, pagenationFilter);

                Pagenation.StatusSearch = pagenationFilter.status;
                Pagenation.Search = pagenationFilter.searchText;
                Pagenation.Data = PageData;

                try
                {
                    var jsonData = JsonConvert.SerializeObject(Pagenation.Data);

                    if (!string.IsNullOrEmpty(jsonData) && !string.IsNullOrWhiteSpace(jsonData))
                    {
                        Pagenation.JsonData = jsonData;
                    }

                    Pagenation.JsonData = "";
                }
                catch (Exception ex)
                {
                    Pagenation.JsonData = "";
                }

                return Pagenation;
            }
            catch (Exception ex)
            {
                return Pagenation;
            }
        }

        public int FindIndex<T>(IEnumerable<T> list, Func<T, bool> predicate)
        {
            var idx = list.Select((value, index) => new { value, index }).Where(x => predicate(x.value)).Select(x => x.index).First();
            return idx;
        }

        private async Task<List<PageMode>> GetPageCount(List<PageMode> PageModes, PagenationFilterDto pagenationFilter)
        {
            try
            {
                if (PageModes.Count >= 6)
                {
                  //  return PageModes.Skip(pagenationFilter.).Take(6).ToList();
                }

                return PageModes.Take(PageModes.Count).ToList();
            }
            catch (Exception ex)
            {
                return PageModes.Take(PageModes.Count).ToList();
            }
        }

        private async Task<long> GetUnderPageCount<p>(List<p> PageModes, PagenationFilterDto pagenationFilter)
        {
            try
            {
                if (PageModes == null)
                {
                    return 0;
                }

                if (PageModes.Count >= 6)
                {
                     return PageModes.Take(6).ToList().Count;
                }

                return PageModes.Take(PageModes.Count).ToList().Count;
            }
            catch (Exception ex) 
            {
                return PageModes.Take(PageModes.Count).ToList().Count;
            }
        }
    }
}
