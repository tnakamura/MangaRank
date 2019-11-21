import React from 'react'

const EntryList = ({ entries }) => {
  return (
    <ul>
      {entries.map(({ node }) => <li key={node.id}>
         <a href={node.url}
            target="_blank"
            rel="noopener noreferrer">
          {node.title}
         </a>
      </li>)}
    </ul>
  )
}

export default EntryList
