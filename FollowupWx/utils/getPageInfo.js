/*获取当前页url*/
const getCurrentPageUrl=()=>{
  let pages = getCurrentPages()    //获取加载的页面
  let currentPage = pages[pages.length-1]    //获取当前页面的对象
  let url = currentPage.route    //当前页面url
  return url
}
/*获取当前页参数*/
const getCurrentPageParam=()=>{
  let pages = getCurrentPages()    //获取加载的页面
  let currentPage = pages[pages.length-1]    //获取当前页面的对象
  let options = currentPage.options    //如果要获取url中所带的参数可以查看options
  return options
}
module.exports = {
  getCurrentPageUrl,
  getCurrentPageParam
}