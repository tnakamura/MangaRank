import React from 'react'
import ItemCell from '../molecules/item-cell'

const ItemList = ({ items, page, perPage }) => {
  return (
    <>
      {items.map((item, i) => {
        const offset = (page - 1) * perPage
        const rank = i + 1 + offset
        const rowClassName = (i > 0) ? "mt-5" : "mt-3"
        return <ItemCell key={item.asin}
                         rank={rank}
                         item={item}
                         rowClassName={rowClassName}/>
      })}
    </>
  )
}

export default ItemList
