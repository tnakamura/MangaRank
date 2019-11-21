import React from 'react'
import TagLink from '../atoms/tag-link'

const TagList = ({ tags, inline }) => {
  const ulClassName = inline ? "list-inline" : "list-unstyled"
  const liClassName = inline ? "list-inline-item": null
  return (
    <ul className={ulClassName}>
      {tags.map(tag => <li key={tag.name} className={liClassName}>
        <TagLink name={tag.name}/>
      </li>)}
    </ul>
  )
}

export default TagList
